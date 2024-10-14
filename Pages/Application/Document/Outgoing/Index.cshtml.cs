using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.OutGoing
{

	[Authorize(Roles = "Admin,Reviewer")]
	public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
       
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IDocumentTrackingRepository _trackingRepository;
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
        private readonly ICHEDPersonelRepository _reviewerRepo;
        private readonly IMapper _mapper;
        private readonly IBaseRepository<Designation> _designationRepo;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            ICHEDPersonelRepository reviewerRepo,
            UserManager<AppIdentityUser> userManager,
            IDocumentTrackingRepository trackingRepository,
            IBaseRepository<AppIdentityUser> userRepo,
            IMapper mapper,
            IBaseRepository<Designation> designationRepo)
        {
            _docRepo = docRepo;
            _reviewerRepo = reviewerRepo;
            _userManager = userManager;
            _mapper = mapper;
            _trackingRepository = trackingRepository;
            _userRepo = userRepo;
            _designationRepo = designationRepo;
        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var docTrackings = await _trackingRepository.GetAll();
            var account = await _userManager.FindByNameAsync(User?.Identity?.Name);
            var getRoles = await _userManager.GetRolesAsync(account);
            var reviewers = await _reviewerRepo.CHEDPersonelRecords();
            var designations = await _designationRepo.GetAll();
            var firstDesingationName = designations.OrderBy(x => x.AddedAt).First().DesignationName;
            var designationName = reviewers.FirstOrDefault(x => x.Account.Id == account.Id && x.Designation != null)?.Designation.DesignationName ?? null;

            var filterRecords = docsAttachments.OrderByDescending(x => x.DocumentTracking.Id).ToList();
            DocsAttachments = docsAttachments
                .Where(x => x.DocumentTrackings.FirstOrDefault(y => y.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Approved && !firstDesingationName.Contains(designationName) && !x.DocumentTrackings.Any(g => g.ReviewerStatus == ReviewerStatus.Completed) || x.DocumentTrackings.FirstOrDefault(y => y.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Passed && !firstDesingationName.Contains(designationName) && !x.DocumentTrackings.Any(g => g.ReviewerStatus == ReviewerStatus.Completed) || x.DocumentTrackings.FirstOrDefault(y => y.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Passed && firstDesingationName.Contains(designationName) && !x.DocumentTrackings.Any(g => g.ReviewerStatus == ReviewerStatus.Completed || g.ReviewerStatus == ReviewerStatus.Approved) || x.DocumentTrackings.FirstOrDefault(y => y.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Reviewed  && x.DocumentTrackings.Any(r => r.ReviewerStatus == ReviewerStatus.Approved) && !x.DocumentTrackings.Any(r => r.ReviewerStatus == ReviewerStatus.Completed) && !firstDesingationName.Contains(designationName) || x.DocumentAttachment.DocumentType == DocumentType.WalkIn && x.DocumentTrackings.Any(e => e.ReviewerId != account.Id) && designationName == firstDesingationName && !x.DocumentTrackings.Any(t => t.ReviewerStatus == ReviewerStatus.Approved))
                .ToList();
                

        }
    }
}
