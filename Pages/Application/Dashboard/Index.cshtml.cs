using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ChartViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docsAttachRepo;
        private readonly IBaseRepository<CHEDPersonel> _chedPRepo;
		private readonly UserManager<AppIdentityUser> _userManager;
		private readonly IBaseRepository<Sender> _senderRepo;
		private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;

        public IndexModel(
            IDocumentAttachmentRepository docsAttachRepo,
            IBaseRepository<CHEDPersonel> chedPRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<Sender> senderRepo,
            IBaseRepository<AppIdentityUser> accRepo,
			IBaseRepository<Notification> notificationRepo,
			IHubContext<NotificationHub, INotificationHub> notifHub)
        {
            _docsAttachRepo = docsAttachRepo;
            _chedPRepo = chedPRepo;
            _userManager = userManager;
            _senderRepo = senderRepo;
            _accRepo = accRepo;
            _notifHub = notifHub;
            _notificationRepo = notificationRepo;
        }
        public int CountChedPersonel { get; set; }
        public int CountRSender { get; set; }
        public int CountIncomingDocs { get; set; }
        public int CountAllDocs { get; set; }
        public int CountPendingDocs { get; set; }
        public int CountOnProcessDocs { get; set; }
        public List<DocumentAttachmentViewModel> RecentDocuments { get; set; }
        public List<AppIdentityUser> RecentSenders { get; set; }

       

        
        public async Task OnGetAsync()
        {
            var chedPersonels = await _chedPRepo.GetAll();
            CountChedPersonel = chedPersonels.Count();
            var Senders = await _senderRepo.GetAll();
            CountRSender = Senders.Count();
            var docsAttachments = await _docsAttachRepo.DocumentAttachments();
            var fDocsAttachments = docsAttachments
               .ToList();
          //  CountIncomingDocs = fDocsAttachments.Where(x =>   (x.DocumentAttachment.Status != Status.Approved && x.DocumentAttachment.Status != Status.Disapproved) && x.ReviewerAccount.UserName == User.Identity?.Name).Count();
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var role = await _userManager.GetRolesAsync(account);
            CountIncomingDocs = docsAttachments
                .Where(s => s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.ToReceived || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.OnReview || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Reviewed && (int)s.DocumentAttachment.Status < 2 || s.DocumentAttachment.Status == Status.PreparingRelease && role.Any(x => x == "Admin"))
                .Count();

            if (User.IsInRole("Sender"))
            {
                CountAllDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name).Count();
                RecentDocuments = fDocsAttachments
                   .Where(x => (x.DocumentAttachment.Status == Status.Approved || x.DocumentAttachment.Status == Status.Disapproved) && x.SenderAccount.UserName == User.Identity?.Name)
                   .OrderByDescending(x => x.DocumentAttachment.UpdatedAt)
                   .Take(5)
                   .ToList();
                CountOnProcessDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name && x.DocumentAttachment.Status == Status.OnProcess).Count();
                CountPendingDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name && x.DocumentAttachment.Status == Status.Pending).Count();
            }

            else
            {
                CountAllDocs = fDocsAttachments.Count();
                RecentDocuments = fDocsAttachments
                   .Where(x => x.DocumentAttachment.Status == Status.Approved || x.DocumentAttachment.Status == Status.Disapproved)
                   .OrderByDescending(x => x.DocumentAttachment.UpdatedAt)
                   .Take(5)
                   .ToList();
            }
               

            RecentSenders = docsAttachments
               .GroupBy(x => x.SenderAccount.Id)
               .Select(result => new AppIdentityUser
               {
                   TypeOfUser = result.First().SenderAccount.TypeOfUser,
                   FirstName = result.First().SenderAccount.FirstName,
                   LastName = result.First().SenderAccount.LastName,
                   MiddleName = result.First().SenderAccount.MiddleName,
                   Suffixes = result.First().SenderAccount.Suffixes,
                   Address = result.First().SenderAccount.Address
               })
               .Take(5)
               .Where(x => x.TypeOfUser != TypeOfUser.Admin)
               .ToList();
        }
        public async Task<JsonResult> OnGetApprovedAndDisApproved()
        {
            var fDocuments = new List<DocumentAttachment>();
            var documents = await _docsAttachRepo.GetAll();
            var account = await _userManager.FindByNameAsync(User.Identity.Name);
            if (User.IsInRole("Sender"))
                fDocuments = documents.Where(x => x.SenderId == account?.Id).ToList();
            else
                    fDocuments = documents.ToList();
            var result = new ApprovedDisapproved()
            {
                CountApproved = fDocuments.Where(x => x.Status == Status.Approved).Count(),
                CountDisapproved = fDocuments.Where(x => x.Status == Status.Disapproved).Count()
            };
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetEndedDocsPerMonth()
        {
            var fDocuments = new List<DocumentAttachment>();
            var documents = await _docsAttachRepo.GetAll();
            var account = await _userManager.FindByNameAsync(User.Identity.Name);
           
            if (User.IsInRole("Sender"))
                fDocuments = documents.Where(x => x.SenderId == account?.Id).ToList();
            else
                fDocuments = documents.ToList();
            int currentYear = DateTime.Now.Year;

            var result = new EndedDocsPerMonthViewModel
            {
                January = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 1 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                February = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 2 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                March = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 3 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                April = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 4 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                May = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 5 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                June = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 6 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                July = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 7 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                August = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 8 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                September = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 9 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                October = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 10 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                November = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 11 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count(),

                December = fDocuments
                    .Where(x => (x.Status == Status.Approved || x.Status == Status.Disapproved) &&
                                x.UpdatedAt.Month == 12 &&
                                x.UpdatedAt.Year == currentYear)
                    .Count()
            };

            return new JsonResult(result);
        }

    }
}
