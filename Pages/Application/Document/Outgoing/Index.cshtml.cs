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
        private readonly IBaseRepository<CHEDPersonel> _reviewerRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IDocumentTrackingRepository _trackingRepository;
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
        private readonly IMapper _mapper;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IBaseRepository<CHEDPersonel> reviewerRepo,
            UserManager<AppIdentityUser> userManager,
            IDocumentTrackingRepository trackingRepository,
            IBaseRepository<AppIdentityUser> userRepo,
            IMapper mapper)
        {
            _docRepo = docRepo;
            _reviewerRepo = reviewerRepo;
            _userManager = userManager;
            _mapper = mapper;
            _trackingRepository = trackingRepository;
            _userRepo = userRepo;

        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var docTrackings = await _trackingRepository.GetAll();
            var account = await _userManager.FindByNameAsync(User?.Identity?.Name);
            var getRoles = await _userManager.GetRolesAsync(account);

            var filterRecords = docsAttachments.OrderByDescending(x => x.DocumentTracking.Id).ToList();
            DocsAttachments = docsAttachments
                .Where(x => x.DocumentAttachment.DocumentTrackings.OrderByDescending(x => x.AddedAt).First().ReviewerId != account.Id && !x.DocumentAttachment.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Approved || x.ReviewerStatus == ReviewerStatus.Disapproved))
                .ToList();
                
                /*filterRecords
               .Where(s => s.DocumentAttachment.DocumentType == DocumentType.WalkIn && getRoles.Any(x => x == "Admin") && (int)s.DocumentTrackings.OrderByDescending(x => x.Id).First().ReviewerStatus < 4 || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Reviewed && (int)s.DocumentAttachment.Status < 3 || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Passed && (int)s.DocumentAttachment.Status < 3)
               .ToList();
                */

        }
    }
}
