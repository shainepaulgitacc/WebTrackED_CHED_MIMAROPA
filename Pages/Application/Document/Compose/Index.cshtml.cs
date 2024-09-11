using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using Microsoft.AspNetCore.Authorization;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;
using Microsoft.AspNetCore.SignalR;
using WebTrackED_CHED_MIMAROPA.Hubs;
using System.ComponentModel.DataAnnotations;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.Compose
{

    public class SendersList
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
    }
    [Authorize(Roles = "Sender,Admin")]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IBaseRepository<DocumentAttachment> _repo;
        private readonly IBaseRepository<Office> _officeRepo;
        private readonly IBaseRepository<DocumentTracking> _docTrackingRepo;
        private readonly ICHEDPersonelRepository _chedRepo;
        private readonly IBaseRepository<Category> _categRepo;
        private readonly IBaseRepository<SubCategory> _scategRepo;
        private readonly IBaseRepository<Procedure> _procedureRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IBaseRepository<Settings> _settingsRepo;
        private readonly IMapper _mapper;
        private readonly FileUploader _fileUploader;
        private readonly IBaseRepository<DocumentProcedure> _docsProcedure;
		private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
		public IndexModel(
            IBaseRepository<DocumentAttachment> repo,
            ICHEDPersonelRepository chedRepo,
            IBaseRepository<Category> categRepo,
            IBaseRepository<Sender> senderRepo,
            IBaseRepository<SubCategory> scategRepo,
            IBaseRepository<Procedure> procedureRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<DocumentTracking> docTrackingRepo,
            IMapper mapper,
            FileUploader fileUploader,
            IBaseRepository<DocumentProcedure> docsProcedure,
			IBaseRepository<Notification> notificationRepo,
			IHubContext<NotificationHub, INotificationHub> notifHub,
            IBaseRepository<Settings> settingsRepo,
            IBaseRepository<AppIdentityUser> userRepo,
            IBaseRepository<Office> officeRepo)
        {
            _repo = repo;
            _chedRepo = chedRepo;
            _categRepo = categRepo;
            _senderRepo = senderRepo;
            _scategRepo = scategRepo;
            _userManager = userManager;
            _procedureRepo = procedureRepo;
            _docTrackingRepo = docTrackingRepo;
            _mapper = mapper;
            _fileUploader = fileUploader;
            _docsProcedure = docsProcedure;
            _notificationRepo = notificationRepo;
            _notifHub = notifHub;
            _settingsRepo = settingsRepo;
            _userRepo = userRepo;
            _officeRepo = officeRepo;
        }
        [BindProperty]
        public ComposeInputModel InputModel { get; set; }
        public List<CHEDPersonelListViewModel> Reviewers { get; set; }
        public string RecordsOfficeId { get; set; }
        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public AppIdentityUser Sender { get; set; }

      

        //public List<AppIdentityUser> Reviewers { get; set; }
        public async Task OnGetAsync()
        {
            var reviewers = await _chedRepo.CHEDPersonelRecords();
            var offices = await _officeRepo.GetAll();
            

            var recordsOfficeId= reviewers.FirstOrDefault(x => x.Office != null && x.Office.Id == offices.Min(x => x.Id))?.Account.Id;
            var categories = await _categRepo.GetAll();
            var subcategories = await _scategRepo.GetAll();
            RecordsOfficeId = recordsOfficeId;
            var senderUser = await _userManager.FindByNameAsync(User.Identity?.Name);
            Sender = senderUser;


			InputModel = new ComposeInputModel()
			{
				SenderId = senderUser?.Id,
                DocumentType = User.IsInRole("Admin")?DocumentType.WalkIn:DocumentType.OnlineSubmission
			};

            Reviewers = reviewers.Where(x => x.Account.Id != senderUser.Id).ToList();
            Categories = categories.ToList();
            SubCategories = subcategories.ToList();
        }
		public async Task<IActionResult> OnPostAsync()
        
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var convert = _mapper.Map<DocumentAttachment>(InputModel);
            convert.FileName = await _fileUploader.Uploadfile(InputModel.File,"Documents");

            await _repo.Add(convert);

            foreach(var revId in InputModel.ReviewersId)
            {
                var docTracking = new DocumentTracking()
                {
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ReviewerStatus = ReviewerStatus.ToReceived,
                    ReviewerId = revId,
                    DocsAttachment = convert
                };

                await _docTrackingRepo.Add(docTracking);

                var users = await _chedRepo.GetAll();
                var user = users.FirstOrDefault(x => x.IdentityUserId == revId);
                var userAcc = await _userManager.FindByIdAsync(user.IdentityUserId);
                var senderAcc = await _userManager.FindByNameAsync(User.Identity.Name);
                var notification = new Notification
                {
                    Recepient = user.IdentityUserId,
                    Title = "New Document Received",
                    Description = User.IsInRole("Sender") ? $"You have received a new document from {senderAcc.FirstName} {senderAcc.MiddleName} {senderAcc.LastName} {senderAcc.Suffixes}. You can now review it." : $"You have received a new document from Records Office. You can now review it.",
                    NotificationType = NotificationType.Document,
                    RedirectLink = "/Application/Document/Incoming/Index",
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                var settings = await _settingsRepo.GetAll();
                if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
                {
                    await _notificationRepo.Add(notification);
                    _notifHub.Clients.User(user.IdentityUserId).ReceiveNotification(notification.Title, notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description, notification.NotificationType.ToString(), notification.AddedAt.ToString("MMMM dd, yyy"), notification.RedirectLink);
                }
            }
           

            
			


            var procedures = await _procedureRepo.GetAll();

            foreach (var procedure in procedures.Where(x => x.SubCategoryId == convert.SubCategoryId))
            {
                await _docsProcedure.Add(new DocumentProcedure()
                {
                    DocumentAttachment = convert,
                    IsDone = false,
                    ProcedureDescription = procedure.Description,
                    ProcedureTitle = procedure.ProcedureTitle

                });
            }


            TempData["validation-message"] = "Successfully submitted";
            if (User.IsInRole("Admin"))
                return RedirectToPage("/Application/Document/Outgoing/Index");
            else
				return RedirectToPage("/Application/Document/Pending/Index");
		}
    }
}
