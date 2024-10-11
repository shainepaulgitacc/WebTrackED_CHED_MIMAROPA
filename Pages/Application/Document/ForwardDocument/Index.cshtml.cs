using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using WebTrackED_CHED_MIMAROPA.Hubs;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.ForwardDocument
{


    public class IndexModel : PageModel
    {
    
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
      
        private readonly IBaseRepository<Designation> _designationRepo;

        public IndexModel(
       
            IBaseRepository<DocumentTracking> docsTrackRepo,
            IBaseRepository<Sender> senderRepo,
            ICHEDPersonelRepository chedPRepo,
            UserManager<AppIdentityUser> userManager,
            IDocumentAttachmentRepository documentAttachmentRepository,
            IHubContext<NotificationHub, INotificationHub> notifHub,
            IBaseRepository<Notification> notificationRepo,
            IBaseRepository<Settings> settingsRepo,
            IBaseRepository<AppIdentityUser> revAccRepo,
          
            IBaseRepository<Designation> designationRepo,

        IMapper mapper)

        {
          
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
          
            _designationRepo = designationRepo;
        }
    
        public List<CHEDPersonelListViewModel> ChedPersonels { get; set; }

        public List<CHEDList> ValidReviewers { get; set; }

        public List<DocumentTracking> DocumentTrackings { get; set; }
        public int PId { get; set; }
        public string OldReviewerId { get; set; }
        public string PreviousPage { get; set; }
        public string ReviewerDesignationName { get; set; }
        public bool CurrentlyToReviewed { get; set; }
        [BindProperty]
        public ForwardDocumentInputModel InputModel { get; set; }

        public string FirstDesignationName { get; set; }
      
        public async Task<IActionResult> OnGetAsync(int pId, string prevPage)
        {
            PId = pId;
            PreviousPage = prevPage;
           
            var chedPersonels = await _chedPRepo.CHEDPersonelRecords();
            var designations = await _designationRepo.GetAll();
            var firstDesignationName = designations.OrderBy(x => x.AddedAt).First().DesignationName;
            var chedP = await _chedPRepo.GetAll();
           

            var docsTrackings = await _docsTrackRepo.GetAll();
            var docAttachments = await _documentAttachmentRepository.GetAll();
            var reviewers = await _revAccRepo.GetAll();

            var docAttachment = await _documentAttachmentRepository.GetOne(pId.ToString());
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);

            ReviewerDesignationName = designations
                .Join(chedP,
                d => d.Id,
                c => c.DesignationId,
                (d, c) => new
                {
                    Designation = d,
                    CHEDP = c
                })
                .FirstOrDefault(x => x.CHEDP.IdentityUserId == account.Id)
                .Designation.DesignationName;

       

            var revs = await _revAccRepo.GetAll();
            var docTracks = await _docsTrackRepo.GetAll();
            var reviewerStatus = docAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id).ReviewerStatus;

            var reviewerDesignationName = chedPersonels.FirstOrDefault(x => x.Account.Id == account.Id).Designation.DesignationName;
            FirstDesignationName = firstDesignationName;
            DocumentTrackings = docAttachment.DocumentTrackings.ToList();

            if (docAttachment.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.PreparingRelease || docAttachment.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.Approved)
            {
                if (reviewerDesignationName.Contains(firstDesignationName))
                {
                    return RedirectToPage("/Application/Document/Incoming/Index");
                }
                return RedirectToPage("/Application/Document/Outgoing/Index");
            }
            else if (docAttachment.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.Completed)
                return RedirectToPage("/Application/Document/Ended/Index");
            else if (reviewerStatus != ReviewerStatus.Reviewed || reviewerStatus == ReviewerStatus.Reviewed && !(docAttachment.DocumentTrackings.First().ReviewerStatus != ReviewerStatus.PreparingRelease || docAttachment.DocumentTrackings.First().ReviewerStatus != ReviewerStatus.Approved || docAttachment.DocumentTrackings.First().ReviewerStatus != ReviewerStatus.Completed))
                return BadRequest("Can't access this page");

            ValidReviewers = revs
               .GroupJoin(docTracks.OrderByDescending(x => x.Id),
               r => r.Id,
               d => d.ReviewerId,
               (r, d) => new
               {
                   Reviewer = r,
                   DocumentTracking = d
               })
               .Join(chedP,
               r => r.Reviewer.Id,
               c => c.IdentityUserId,
               (r, c) => new
               {
                   Reviewer = r.Reviewer,
                   DocumentTracking = r.DocumentTracking,
                   ChedPersonel = c
               })
               .Join(designations,
                d => d.ChedPersonel.DesignationId,
                c => c.Id,
                (d,c) => new
                {
					Reviewer = d.Reviewer,
					DocumentTracking = d.DocumentTracking.FirstOrDefault(),
					ChedPersonel = d.ChedPersonel,
                    Designation = c
				})
               .Select(result => new
               {
                   Reviewer = result.Reviewer,
                   DocumentTracking = result.DocumentTracking,
                   ChedPersonel = result.ChedPersonel,
               
                   Designation = result.Designation
               })
               .Where(x =>  !docsTrackings.Any(w => w.DocumentAttachmentId == pId && w.ReviewerId == x.Reviewer.Id) || x.Designation != null && x.Designation.DesignationName.Contains("Records") && x.DocumentTracking == null || x.Designation != null && x.DocumentTracking.ReviewerId != account.Id && !(docAttachments.FirstOrDefault(x => x.Id == pId).DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(y => y.ReviewerId == x.Reviewer.Id).ReviewerStatus == ReviewerStatus.ToReceived || docAttachments.FirstOrDefault(x => x.Id == pId).DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(y => y.ReviewerId == x.Reviewer.Id).ReviewerStatus == ReviewerStatus.OnReview))
               .GroupBy(res => res.Reviewer.Id)
               .Select(result => new CHEDList
               {
                   User = result.First().Reviewer,
                   Designation =result.First().Designation
               })
               .ToList();

               CurrentlyToReviewed = docAttachment.DocumentTrackings
                .OrderByDescending(x => x.Id)
                .GroupBy(x => x.ReviewerId)
                .Select(r => new
                {
                    ReviewerId = r.Key,
                    HasCurrentlyOnReview = r.First().ReviewerStatus == ReviewerStatus.ToReceived || r.First().ReviewerStatus == ReviewerStatus.OnReview
                })
                .Where(x => x.HasCurrentlyOnReview)
                .Count() > 0;

            OldReviewerId = chedPersonels.FirstOrDefault(x => x.Account.Id == account?.Id).CHEDPersonel.IdentityUserId;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string pId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var documentAttachment = await _documentAttachmentRepository.GetOne(pId);
            var Reviewers = await _chedPRepo.CHEDPersonelRecords();
            var newRevAcc = new CHEDPersonelListViewModel();
            var cUser =  Reviewers.FirstOrDefault(x => x.Account.UserName == User.Identity.Name);
            

            var settings = await _settingsRepo.GetAll();
            var setting = settings.OrderByDescending(x => x.Id).First();


            var docTrackings = await _docsTrackRepo.GetAll();
            var docAttachment = await _documentAttachmentRepository.GetAll();
          
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            foreach (var rev in  documentAttachment.DocumentTrackings.Where(x => x.ReviewerId != user.Id))
            {
				_notifHub.Clients.User(rev.ReviewerId).ReviewerRealtime();

               	
			}
            await _documentAttachmentRepository.Update(documentAttachment,pId);

            await _docsTrackRepo.Add(new DocumentTracking
            {
                AddedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Note = InputModel.Note,
                ReviewerId = cUser.Account.Id,
                DocumentAttachmentId = InputModel.DocumentId,
                ReviewerStatus = ReviewerStatus.Passed
            });

             foreach (var reviewer in InputModel.NewReviewers.Split(","))
            {
                newRevAcc = Reviewers.FirstOrDefault(x => x.CHEDPersonel.IdentityUserId == reviewer);
                if (setting.DocumentNotif)
                {
                    await _docsTrackRepo.Add(new DocumentTracking
                    {
                        AddedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        ReviewerId = reviewer,
                        DocumentAttachmentId = InputModel.DocumentId,
                        ReviewerStatus = InputModel.TrackingStatus,
                    });
                    // Notification for the sender when the document is passed to the next reviewer for sender
                    /*
                    var notificationPassed = new Notification
                    {
                        Title = "Document Passed for Review",
                        Recepient = documentAttachment.SenderId,
                        IsViewed = false,
                        IsArchived = false,
                        Description = $"Your document has been passed to {newRevAcc.Account.FirstName} {newRevAcc?.Account.MiddleName} {newRevAcc?.Account.LastName} {newRevAcc?.Account.Suffixes})",
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
                    */
                    /*
                    //notification for the next reviewer
                    var notificationPassedReviewer = new Notification
                    {
                        Title = InputModel.TrackingStatus == ReviewerStatus.PreparingRelease ?
                               "Document Ready for Release" :
                               "Document Review Required",
                        Recepient = newRevAcc?.Account.Id,
                        IsViewed = false,
                        IsArchived = false,
                        Description = InputModel.TrackingStatus == ReviewerStatus.PreparingRelease ?
                                 "This document is now ready for release. Please proceed with your review." :
                                 $"You have received a new document from {cUser?.Account.FirstName} {cUser.Account.MiddleName} {cUser.Account.LastName} {cUser.Account.Suffixes} ({cUser.Designation.DesignationName}). Kindly review the document at your convenience.",
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
                    */

                }



            }
            TempData["validation-message"] = "Successfully passed to another reviewer";
            return RedirectToPage("/Application/Document/OutGoing/Index");
        }

    }
}
