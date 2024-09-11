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
        private readonly IMapper _mapper;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IDocumentTrackingRepository docTrackRepo,
            IBaseRepository<CHEDPersonel> reviewerRepo,
            UserManager<AppIdentityUser> userManager,
            IMapper mapper)
        {
            _docRepo = docRepo;
            _docTrackRepo = docTrackRepo;
            _reviewerRepo = reviewerRepo;
            _userManager = userManager;
            _mapper = mapper;
        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();

            var account = await _userManager.FindByNameAsync(User.Identity?.Name);

            var role = await _userManager.GetRolesAsync(account);
            DocsAttachments = docsAttachments
                .Where(s => s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.ToReceived || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.OnReview  || s.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Reviewed && (int)s.DocumentAttachment.Status < 2 || s.DocumentAttachment.Status == Status.PreparingRelease && role.Any(x => x == "Admin"))
                .ToList();


            

        }
    }
}
