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
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IDocumentTrackingRepository docTrackRepo,
            IBaseRepository<CHEDPersonel> reviewerRepo,
            UserManager<AppIdentityUser> userManager,
            IMapper mapper,
            ICHEDPersonelRepository chedRepo)
        {
            _docRepo = docRepo;
            _docTrackRepo = docTrackRepo;
            _reviewerRepo = reviewerRepo;
            _userManager = userManager;
            _mapper = mapper;
            _chedRepo = chedRepo;
        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var account = await _userManager.FindByNameAsync(User.Identity?.Name);
            var reviewerRecord = await _chedRepo.CHEDPersonelRecords();
            var reviewer = reviewerRecord.FirstOrDefault(x => x.Account.UserName == User.Identity.Name);

            var role = await _userManager.GetRolesAsync(account);
            DocsAttachments = docsAttachments
            .Where(x => reviewer.Office.OfficeName.Contains("Records Office") && x.DocumentAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.PreparingRelease || x.DocumentAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.ToReceived || x.DocumentAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.OnReview || x.DocumentAttachment.DocumentTrackings.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ReviewerId == account.Id)?.ReviewerStatus == ReviewerStatus.Reviewed && !x.DocumentAttachment.DocumentTrackings.Any(b => b.ReviewerStatus == ReviewerStatus.PreparingRelease))

             
            .ToList();


            

        }
    }
}
