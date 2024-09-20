using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Numerics;
using System.Security.Policy;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;
using WebTrackED_CHED_MIMAROPA.Pages.Application.Usermanagement.CHEDPersonel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.Incoming
{

    [Authorize(Roles = "Admin,Reviewer")]
    [ValidateAntiForgeryToken]
    public class ProcedureCheckingModel : PageModel
    {
        private readonly IBaseRepository<DocumentProcedure> _docsProcedRepo;
        private readonly IBaseRepository<DocumentTracking> _docsTrackRepo;
        private readonly ICHEDPersonelRepository _chedPRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly IBaseRepository<AppIdentityUser> _revAccRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IDocumentAttachmentRepository _documentAttachmentRepository;
        private readonly IHubContext<NotificationHub, INotificationHub> _notifHub;
        private readonly IBaseRepository<Notification> _notificationRepo;
        private readonly IBaseRepository<Settings> _settingsRepo;
        private readonly IMapper _mapper;
        public ProcedureCheckingModel(
            IBaseRepository<DocumentProcedure> docsProcedRepo,
            IBaseRepository<DocumentTracking> docsTrackRepo,
            IBaseRepository<Sender> senderRepo,
            ICHEDPersonelRepository chedPRepo,
            UserManager<AppIdentityUser> userManager,
            IDocumentAttachmentRepository documentAttachmentRepository,
            IHubContext<NotificationHub, INotificationHub> notifHub,
            IBaseRepository<Notification> notificationRepo,
            IBaseRepository<Settings> settingsRepo,
            IBaseRepository<AppIdentityUser> revAccRepo,

        IMapper mapper)

        {
            _docsProcedRepo = docsProcedRepo;
            _docsTrackRepo = docsTrackRepo;
            _senderRepo = senderRepo;
            _chedPRepo = chedPRepo;
            _userManager = userManager;
            _documentAttachmentRepository = documentAttachmentRepository;
            _mapper = mapper;
            _notifHub = notifHub;
            _notificationRepo = notificationRepo;
            _settingsRepo = settingsRepo;
            _revAccRepo = revAccRepo;
        }
        public List<DocumentProcedure> documentProcedures { get; set; }
        public List<CHEDPersonelListViewModel> ChedPersonels { get; set; }

        public List<CHEDList> ValidReviewers { get; set; }
        public List<string> NewReviewers { get; set; }
        public int PId { get; set; }
        public string OldReviewerId { get; set; }
        public string PreviousPage { get; set; }


        public bool HasCurrentllyReviewing { get; set; }

        public bool PrioritizationIsNull { get; set; }

        [BindProperty]
        public ProcedureCheckingInputModel InputModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int pId, string prevPage)
        {
            PId = pId;
            PreviousPage = prevPage;
            var docProcedures = await _docsProcedRepo.GetAll();
            var chedPersonels = await _chedPRepo.CHEDPersonelRecords();

            var docsTrackings = await _docsTrackRepo.GetAll();
            var docAttachments = await _documentAttachmentRepository.GetAll();
            var reviewers = await _revAccRepo.GetAll();

            var docAttachment = await _documentAttachmentRepository.GetOne(pId.ToString());
            PrioritizationIsNull = docAttachment.Prioritization == null;

            if (docAttachment.Status == Status.PreparingRelease || docAttachment.Status == Status.Approved || docAttachment.Status == Status.Disapproved)
                return RedirectToPage("/Application/Document/Outgoing/Index");


            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var revs = await _revAccRepo.GetAll();
            var docTracks = await _docsTrackRepo.GetAll();


            var filtered = docProcedures.Where(x => x.DocumentAttachmentId == pId).ToList();
            if (filtered.Count() <= 0)
                return BadRequest($"No procedure for now");
            documentProcedures = filtered;




            HasCurrentllyReviewing = docAttachments.Where(x => x.Id == pId)
                .Join(docsTrackings,
                da => da.Id,
                dt => dt.DocumentAttachmentId,
                (da, dt) => new
                {
                    Document = da,
                    DocumentTracking = dt
                })
                .Join(reviewers,
                dt => dt.DocumentTracking.ReviewerId,
                r => r.Id,
                (dt, r) => new
                {
                    Document = dt.Document,
                    DocumentTracking = dt.DocumentTracking,
                    Reviewer = r
                })
                .GroupBy(r => r.Reviewer.Id)
                .Select(result => new
                {
                    ReviewerStatus = result.OrderByDescending(x => x.DocumentTracking.Id).First().DocumentTracking.ReviewerStatus
                })
                .Any(x =>  x.ReviewerStatus == ReviewerStatus.ToReceived|| x.ReviewerStatus == ReviewerStatus.OnReview);


            ValidReviewers = revs
               .GroupJoin(docTracks,
               r => r.Id,
               d => d.ReviewerId,
               (r, d) => new
               {
                   Reviewer = r,
                   DocumentTracking = d
               })

               .Select(result => new
               {
                   Reviewer = result.Reviewer,
                   DocumentTracking = result.DocumentTracking.FirstOrDefault()
               })
               .Where(x => x.DocumentTracking == null && x.Reviewer.TypeOfUser != TypeOfUser.Admin || x.DocumentTracking?.ReviewerId != account.Id && x.Reviewer.TypeOfUser != TypeOfUser.Admin)
               .GroupBy(res => res.Reviewer.Id)
               .Select(result => new CHEDList
               {
                   User = result.First().Reviewer,
                   IsValid = !docTracks.Any( x => x.DocumentAttachmentId == pId &&  x.ReviewerId == result.Key && (int)x.ReviewerStatus < 4),
               })
               .Where(x => x.IsValid && x.User.TypeOfUser != TypeOfUser.Sender)
               .ToList();


            OldReviewerId = chedPersonels.FirstOrDefault(x => x.Account.Id == account?.Id).CHEDPersonel.IdentityUserId;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string pId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var docsProcedures = await _docsProcedRepo.GetAll();
            if (!string.IsNullOrEmpty(InputModel.SelectedIds))
            {
                foreach (var docprocedure in docsProcedures)
                {
                    var splitedIds = InputModel.SelectedIds.Split(",");
                    docprocedure.IsDone = splitedIds.Any(x => x == docprocedure.Id.ToString());
                    await _docsProcedRepo.Update(docprocedure, docprocedure.Id.ToString());
                }
            }
            var documentAttachment = await _documentAttachmentRepository.GetOne(pId);
            var Reviewers = await _chedPRepo.CHEDPersonelRecords();
            var oldRevAcc = Reviewers.FirstOrDefault(x => x.CHEDPersonel.IdentityUserId == InputModel.OldReviewer);
            var newRevAcc = new CHEDPersonelListViewModel();
            var cUser = await _userManager.FindByNameAsync(User.Identity.Name);


            var settings = await _settingsRepo.GetAll();
            var setting = settings.OrderByDescending(x => x.Id).First();


            var docTrackings = await _docsTrackRepo.GetAll();
            var docAttachment = await _documentAttachmentRepository.GetAll();

            var joined1 = docAttachment
                .Join(docTrackings,
                da => da.Id,
                dt => dt.DocumentAttachmentId,
                (da, dt) => new
                {
                    DocumentAttachment = da,
                    DocumentTracking = dt
                })

                .ToList();

            var user = await _userManager.FindByNameAsync(User.Identity.Name);


            foreach (var rev in joined1.Where(x => x.DocumentTracking.DocumentAttachmentId == int.Parse(pId) && x.DocumentTracking.ReviewerId != user.Id))
            {
                _notifHub.Clients.User(rev.DocumentTracking.ReviewerId).ReviewerRealtime();
            }

            await _docsTrackRepo.Add(new DocumentTracking
            {
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ReviewerId = cUser.Id,
                DocumentAttachmentId = InputModel.DocumentId,
                ReviewerStatus = ReviewerStatus.Passed
            });

            foreach (var reviewer in InputModel.NewReviewers.Split(","))
            {
               
                await _docsTrackRepo.Add(new DocumentTracking
                {
                    AddedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ReviewerId = reviewer,
                    DocumentAttachmentId = InputModel.DocumentId,
                    ReviewerStatus = ReviewerStatus.ToReceived
                });

                newRevAcc = Reviewers.FirstOrDefault(x => x.CHEDPersonel.IdentityUserId == reviewer);
                if (setting.DocumentNotif)
                {
                    // Notification for the sender when the document is passed to the next reviewer for sender

                    var notificationPassed = new Notification
                    {
                        Title = "Document Passed for Review",
                        Recepient = documentAttachment.SenderId,
                        IsViewed = false,
                        IsArchived = false,
                        Description = $"Your document has been passed to {newRevAcc.Account.FirstName} {newRevAcc?.Account.MiddleName} {newRevAcc?.Account.LastName} {newRevAcc?.Account.Suffixes}, from ({newRevAcc?.Office.OfficeName})",
                        NotificationType = NotificationType.Document,
                        RedirectLink = documentAttachment.DocumentType != DocumentType.WalkIn ? "/Application/Document/Onprocess/Index" : "/Application/Document/Outgoing/Index",
                        AddedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    await _notificationRepo.Add(notificationPassed);
                    _notifHub.Clients.User(documentAttachment.SenderId).ReceiveNotification(
                        notificationPassed.Title,
                        notificationPassed.Description.Length > 30 ? $"{notificationPassed.Description.Substring(0, 30)}..." : notificationPassed.Description,
                        notificationPassed.NotificationType.ToString(),
                        notificationPassed.AddedAt.ToString("MMMM dd, yyyy"),
                        notificationPassed.RedirectLink
                    );





                    //notification for the next reviewer
                    var notificationPassedReviewer = new Notification
                    {
                        Title = "Review Document",
                        Recepient = newRevAcc?.Account.Id,
                        IsViewed = false,
                        IsArchived = false,
                        Description = $"The document has been forwarded to you by {oldRevAcc?.Account.FirstName} {oldRevAcc?.Account.MiddleName} {oldRevAcc?.Account.LastName} {oldRevAcc?.Account.Suffixes}, from {oldRevAcc?.Office.OfficeName}. You can now review it.",
                        NotificationType = NotificationType.Document,
                        RedirectLink = "/Application/Document/Incoming/Index",
                        AddedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    await _notificationRepo.Add(notificationPassedReviewer);
                    _notifHub.Clients.User(notificationPassedReviewer.Recepient).ReceiveNotification(
                    notificationPassedReviewer.Title,
                        notificationPassedReviewer.Description.Length > 30 ? $"{notificationPassedReviewer.Description.Substring(0, 30)}..." : notificationPassedReviewer.Description,
                        notificationPassedReviewer.NotificationType.ToString(),
                        notificationPassedReviewer.AddedAt.ToString("MMMM dd, yyyy"),
                        notificationPassedReviewer.RedirectLink
                    );

                }



            }
            TempData["validation-message"] = "Successfully passed to another reviewer";
            return RedirectToPage("/Application/Document/OutGoing/Index");
        }
    
        public async Task<IActionResult> OnGetDoneDocument(string pId)
        {
            var docattachment = await _documentAttachmentRepository.GetOne(pId);


          
            if(docattachment == null)
                return BadRequest($"{pId} this document attachment id is not found");

            else if(docattachment.Status == Status.PreparingRelease)
            {
                return RedirectToPage("/Application/Document/Outgoing/Index");
            }


            var docTrackings = await _docsTrackRepo.GetAll();
            var docAttachment = await _documentAttachmentRepository.GetAll();
            var joined1 = docAttachment
                .Join(docTrackings,
                da => da.Id,
                dt => dt.DocumentAttachmentId,
                (da, dt) => new
                {
                    DocumentAttachment = da,
                    DocumentTracking = dt
                })
               
                .ToList();

            var user = await _userManager.FindByNameAsync(User.Identity.Name);


            foreach(var rev in joined1.Where(x => x.DocumentTracking.DocumentAttachmentId == int.Parse(pId) && x.DocumentTracking.ReviewerId != user.Id))
            {
                _notifHub.Clients.User(rev.DocumentTracking.ReviewerId).ReviewerRealtime();
            }


            docattachment.Status = Status.PreparingRelease;
            docattachment.UpdatedAt = DateTime.Now;
            var docsProcedures = await _docsProcedRepo.GetAll();
            var filteredDocProced = docsProcedures.Where(x => x.DocumentAttachmentId == docattachment.Id).ToList();
            var reviewers = await _chedPRepo.CHEDPersonelRecords();
            var reviewer = reviewers.FirstOrDefault(x => x.Office != null && x.Office.OfficeName.Contains("Records Office"))?.Account;

            foreach(var result in filteredDocProced)
            {
                result.IsDone = true;
                await _docsProcedRepo.Update(result,result.Id.ToString());
            }
            await _documentAttachmentRepository.Update(docattachment,docattachment.Id.ToString());


            await _docsTrackRepo.Add(new DocumentTracking
            {
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ReviewerId = reviewer.Id,
                DocumentAttachmentId = int.Parse(pId),
                ReviewerStatus = ReviewerStatus.PreparingRelease
            });




            var documentAttachments = await _documentAttachmentRepository.GetAll();
            var documentTrackings = await _docsTrackRepo.GetAll();
            var joined = documentAttachments
                .Join(documentTrackings,
                da => da.Id,
                dt => dt.DocumentAttachmentId,
                (da, dt) => new
                {
                    DAttachment = da,
                    DTracking = dt
                })
                .GroupBy(x => x.DAttachment.Id)
                .Select(r => new
                {
                    DocumentAttachment = r.First().DAttachment,
                    DocumentTracking = r.OrderByDescending(x => x.DTracking.Id).First().DTracking,
                });

            var notification = new Notification
            {
                Title = "Document Ready for Release",
                Recepient = joined.FirstOrDefault(x => x.DocumentAttachment.Id.ToString() == pId)?.DocumentAttachment.SenderId,
                IsViewed = false,
                Description = "Your document is now ready for release. Check the status in document tracking.",
                NotificationType = NotificationType.Document,
                RedirectLink = docattachment.DocumentType != DocumentType.WalkIn ? "/Application/Document/Onprocess/Index" : "/Application/Document/Outgoing/Index",
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var revNotif = new Notification
            {
                Title = "Document Reviewed and Ready for Release",
                Recepient = joined.FirstOrDefault(x => x.DocumentAttachment.Id.ToString() == pId)?.DocumentTracking.ReviewerId,
                IsViewed = false,
                Description = "The document has been reviewed and is ready for release. Check the document status.",
                NotificationType = NotificationType.Document,
                RedirectLink = "/Application/Document/Incoming/Index",
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var settings = await _settingsRepo.GetAll();
            if (settings.OrderByDescending(x => x.Id).First().DocumentNotif)
            {
                await _notificationRepo.Add(notification);
                _notifHub.Clients.User(notification.Recepient).ReceiveNotification(notification.Title, notification.Description.Length > 30 ? $"{notification.Description.Substring(0, 30)}..." : notification.Description, notification.NotificationType.ToString(), notification.AddedAt.ToString("MMMM dd, yyy"), notification.RedirectLink);
                
                await _notificationRepo.Add(revNotif);
                _notifHub.Clients.User(revNotif.Recepient).ReceiveNotification(revNotif.Title, revNotif.Description.Length > 30 ? $"{revNotif.Description.Substring(0, 30)}..." : revNotif.Description, revNotif.NotificationType.ToString(), revNotif.AddedAt.ToString("MMMM dd, yyy"), revNotif.RedirectLink);

            }
			TempData["validation-message"] = "Successfully approved";
            if (User.IsInRole("Admin"))
                return RedirectToPage("/Application/Document/Incoming/Index");
            return RedirectToPage("/Application/Document/Outgoing/Index");
        }
    }
}
