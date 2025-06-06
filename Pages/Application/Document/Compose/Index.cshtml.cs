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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.CodeAnalysis.Elfie.Serialization;

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
    [Authorize]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IBaseRepository<DocumentAttachment> _repo;
    
        private readonly IBaseRepository<DocumentTracking> _docTrackingRepo;
        private readonly ICHEDPersonelRepository _chedRepo;
        private readonly IBaseRepository<Category> _categRepo;
        private readonly IBaseRepository<SubCategory> _scategRepo;
       
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IBaseRepository<Settings> _settingsRepo;
        private readonly IMapper _mapper;
        private readonly FileUploader _fileUploader;
   
		private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
        private readonly IBaseRepository<Designation> _desigRepo;

		public IndexModel(
            IBaseRepository<DocumentAttachment> repo,
            ICHEDPersonelRepository chedRepo,
            IBaseRepository<Category> categRepo,
            IBaseRepository<Sender> senderRepo,
            IBaseRepository<SubCategory> scategRepo,
          
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<DocumentTracking> docTrackingRepo,
            IMapper mapper,
            FileUploader fileUploader,
          
			IBaseRepository<Notification> notificationRepo,
			IHubContext<NotificationHub, INotificationHub> notifHub,
            IBaseRepository<Settings> settingsRepo,
            IBaseRepository<AppIdentityUser> userRepo,
            IBaseRepository<Designation> desigRepo
           )
        {
            _repo = repo;
            _chedRepo = chedRepo;
            _categRepo = categRepo;
            _senderRepo = senderRepo;
            _scategRepo = scategRepo;
            _userManager = userManager;
          
            _docTrackingRepo = docTrackingRepo;
            _mapper = mapper;
            _fileUploader = fileUploader;
         
            _notificationRepo = notificationRepo;
            _notifHub = notifHub;
            _settingsRepo = settingsRepo;
            _userRepo = userRepo;
            _desigRepo = desigRepo;
           
        }
        [BindProperty]
        public ComposeInputModel InputModel { get; set; }
        public List<CHEDPersonelListViewModel> Reviewers { get; set; }
        public string RecordsDesignationId { get; set; }
        public List<Category> Categories { get; set; }
        public List<SubCategory> SubCategories { get; set; }
        public AppIdentityUser Sender { get; set; }

      

        //public List<AppIdentityUser> Reviewers { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var reviewers = await _chedRepo.CHEDPersonelRecords();
         
         
            var categories = await _categRepo.GetAll();
            var subcategories = await _scategRepo.GetAll();
         
            var senderUser = await _userManager.FindByNameAsync(User.Identity?.Name);
            var designations = await _desigRepo.GetAll();
            var firstDesignation = designations.OrderBy(x => x.AddedAt).First();
            RecordsDesignationId = reviewers.FirstOrDefault(x => x.Designation!= null && x.Designation.Id == firstDesignation.Id)?.Account.Id;

            if (!User.IsInRole("Sender") && !reviewers.FirstOrDefault(x => x.Account.UserName == User.Identity.Name).Designation.DesignationName.Contains(firstDesignation.DesignationName))
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
            Sender = senderUser;

			InputModel = new ComposeInputModel()
			{
				SenderId = senderUser?.Id,
                DocumentType = User.IsInRole("Admin")?DocumentType.WalkIn:DocumentType.OnlineSubmission
			};

            Categories = categories.ToList();
            SubCategories = subcategories.ToList();
            Reviewers = reviewers.Where(x => x.Account.Id != senderUser?.Id && x.Designation != null).ToList();

            return Page();
        }
		public async Task<IActionResult> OnPostAsync()
        
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var convert = _mapper.Map<DocumentAttachment>(InputModel);
            var reviewers = await _chedRepo.CHEDPersonelRecords();
           

            convert.FileName = await _fileUploader.Uploadfile(InputModel.File,"Documents");
            await _repo.Add(convert);
				foreach (var revId in InputModel.ReviewersId.Split(','))
                {
                    var docTracking = new DocumentTracking()
                    {
                        AddedAt = DateTime.UtcNow.AddHours(8),
                        UpdatedAt = DateTime.UtcNow.AddHours(8),
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
                        AddedAt = DateTime.UtcNow.AddHours(8),
                        UpdatedAt = DateTime.UtcNow.AddHours(8),
                    };
                    var settings = await _settingsRepo.GetAll();
                    if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
                    {
                        await _notificationRepo.Add(notification);
                        _notifHub.Clients.User(user.IdentityUserId).ReceiveNotification(notification.Title, notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description, notification.NotificationType.ToString(), notification.AddedAt.ToString("MMMM dd, yyy"), notification.RedirectLink);
                    }
                }

           

            TempData["validation-message"] = "Successfully submitted";
            if (User.IsInRole("Admin"))
                return RedirectToPage("/Application/Document/Outgoing/Index");
            else
				return RedirectToPage("/Application/Document/Pending/Index");
		}
    }
}
