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
        private readonly ICHEDPersonelRepository _chedPRepo;
		private readonly UserManager<AppIdentityUser> _userManager;
		private readonly IBaseRepository<Sender> _senderRepo;
		private readonly IBaseRepository<AppIdentityUser> _accRepo;
        private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
        private readonly IBaseRepository<Designation> _desigRepo;

        public IndexModel(
            IDocumentAttachmentRepository docsAttachRepo,
            ICHEDPersonelRepository chedPRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<Sender> senderRepo,
            IBaseRepository<AppIdentityUser> accRepo,
			IBaseRepository<Notification> notificationRepo,
			IHubContext<NotificationHub, INotificationHub> notifHub,
            IBaseRepository<Designation> desigRepo)
        {
            _docsAttachRepo = docsAttachRepo;
            _chedPRepo = chedPRepo;
            _userManager = userManager;
            _senderRepo = senderRepo;
            _accRepo = accRepo;
            _notifHub = notifHub;
            _notificationRepo = notificationRepo;
            _desigRepo = desigRepo;
        }
        public int CountChedPersonel { get; set; }
        public int CountRSender { get; set; }
        public int CountIncomingDocs { get; set; }
        public int CountAllDocs { get; set; }
        public int CountPendingDocs { get; set; }
        public int CountOnProcessDocs { get; set; }
        public List<DocumentAttachmentViewModel> RecentDocuments { get; set; }
        public List<AppIdentityUser> RecentSenders { get; set; }
        private bool CReviewerStatus(List<DocumentTracking> trackings, string userId,ReviewerStatus status)
        {
			return trackings.FirstOrDefault(x => x.ReviewerId == userId)!= null && trackings.FirstOrDefault(x => x.ReviewerId == userId)?.ReviewerStatus == status;
        }
        public async Task OnGetAsync()
        {
            var chedPersonels = await _chedPRepo.GetAll();
            CountChedPersonel = chedPersonels.Count() - 1;
            var Senders = await _senderRepo.GetAll();
            CountRSender = Senders.Count();
            var docsAttachments = await _docsAttachRepo.DocumentAttachments();
            var fDocsAttachments = docsAttachments
               .ToList();
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var role = await _userManager.GetRolesAsync(account);
            if (User.IsInRole("Sender"))
            {
                CountAllDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name).Count();
                RecentDocuments = fDocsAttachments
                   .Where(x => (x.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.Completed) && x.SenderAccount.UserName == User.Identity?.Name)
                   .OrderByDescending(x => x.DocumentTrackings.First().AddedAt)
                   .Take(5)
                   .ToList();
                CountOnProcessDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name && x.DocumentTrackings.Count() > 1 && !x.DocumentTrackings.Any(r => r.ReviewerStatus == ReviewerStatus.Completed)).Count();
                CountPendingDocs = fDocsAttachments.Where(x => x.SenderAccount.UserName == User.Identity?.Name && x.DocumentTrackings.Count() <=1 ).Count();
            }

            else
            {

                var reviewerRecord = await _chedPRepo.CHEDPersonelRecords();
                var reviewer = reviewerRecord.FirstOrDefault(x => x.Account.UserName == User.Identity.Name);
                var designations = await _desigRepo.GetAll();
                var firstDesignationName = designations.OrderBy(x => x.AddedAt).First().DesignationName;
                CountIncomingDocs = docsAttachments
                                   .Where(x => CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.ToReceived) || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.OnReview) || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.Reviewed) && !x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Approved) || x.DocumentTracking.ReviewerStatus == ReviewerStatus.Approved && reviewer.Designation.DesignationName == firstDesignationName || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.PreparingRelease))
                                   .Count();
                CountAllDocs = fDocsAttachments.Count();
                RecentDocuments = fDocsAttachments
                   .Where(x => x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed))
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
            var result = new
            {
                CountWalkIn = fDocuments.Where(x => x.DocumentType == DocumentType.WalkIn).Count(),
                CountElectronic = fDocuments.Where(x => x.DocumentType == DocumentType.OnlineSubmission).Count()
            };
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetEndedDocsPerMonth()
        {
           // var fDocuments = new List<DocumentAttachment>();
            var docsAttachments = await _docsAttachRepo.DocumentAttachments();
            //var documents = await _docsAttachRepo.GetAll();
            var account = await _userManager.FindByNameAsync(User.Identity.Name);
           
            if (User.IsInRole("Sender"))
                docsAttachments.Where(x => x.DocumentAttachment.SenderId == account?.Id).ToList();
            else
                docsAttachments.ToList();
            int currentYear = DateTime.Now.Year;

            var result = new EndedDocsPerMonthViewModel
            {
                January = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 1 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                February = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 2 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                March = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 3 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                April = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 4 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                May = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 5 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                June = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 6 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                July = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 7 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                August = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 8 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                September = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 9 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                October = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 10 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                November = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 11 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),

                December = docsAttachments
                    .Where(x => (x.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed)) &&
                                x.DocumentTrackings.First().UpdatedAt.Month == 12 &&
                                x.DocumentTracking.UpdatedAt.Year == currentYear)
                    .Count(),
            };

            return new JsonResult(result);
        }

    }
}
