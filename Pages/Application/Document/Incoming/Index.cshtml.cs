using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Admin.Document.Incoming
{

    [Authorize(Roles = "Admin,Reviewer")]
    public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        private readonly IDocumentTrackingRepository _docTrackRepo;
        private readonly IBaseRepository<CHEDPersonel> _reviewerRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly ICHEDPersonelRepository _chedRepo;
        private readonly IMapper _mapper;
        private readonly IBaseRepository<Designation> _desigRepo;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IDocumentTrackingRepository docTrackRepo,
            IBaseRepository<CHEDPersonel> reviewerRepo,
            UserManager<AppIdentityUser> userManager,
            IMapper mapper,
            ICHEDPersonelRepository chedRepo,
            IBaseRepository<Designation> desigRepo)
        {
            _docRepo = docRepo;
            _docTrackRepo = docTrackRepo;
            _reviewerRepo = reviewerRepo;
            _userManager = userManager;
            _mapper = mapper;
            _chedRepo = chedRepo;
            _desigRepo = desigRepo;
        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var reviewerRecord = await _chedRepo.CHEDPersonelRecords();
            var reviewer = reviewerRecord.FirstOrDefault(x => x.Account.UserName == User.Identity.Name);
            var designations = await _desigRepo.GetAll();
            var firstDesignationName = designations.OrderBy(x => x.AddedAt).First().DesignationName;

            var role = await _userManager.GetRolesAsync(account);
            DocsAttachments = docsAttachments
            .Where(x => CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.ToReceived) || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.OnReview) || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.Reviewed) || x.DocumentTracking.ReviewerStatus == ReviewerStatus.Approved && reviewer.Designation.DesignationName == firstDesignationName || CReviewerStatus(x.DocumentTrackings, account.Id, ReviewerStatus.PreparingRelease))
			.ToList();
        }
		private bool CReviewerStatus(List<DocumentTracking> trackings, string userId, ReviewerStatus status)
		{
			return trackings.FirstOrDefault(x => x.ReviewerId == userId) != null && trackings.FirstOrDefault(x => x.ReviewerId == userId)?.ReviewerStatus == status;
		}
	}
}
