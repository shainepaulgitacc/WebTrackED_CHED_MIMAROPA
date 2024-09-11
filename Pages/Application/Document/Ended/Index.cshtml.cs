using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Admin.Document.Ended
{
    [Authorize]
    public class IndexModel : BasePageModel<DocumentAttachment, DocumentAttachmentInputModel>
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        private readonly IDocumentTrackingRepository _docTrackRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly SignInManager<AppIdentityUser> _signInManager;
        private readonly IMapper _mapper;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IDocumentTrackingRepository docTrackRepo,
            IBaseRepository<Sender> senderRepo,
            UserManager<AppIdentityUser> userManager,
            SignInManager<AppIdentityUser> signInManager,
            IMapper mapper) : base(docRepo, mapper)
        {
            _docRepo = docRepo;
            _docTrackRepo = docTrackRepo;
            _senderRepo = senderRepo;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        public List<DocumentAttachmentViewModel> DocsAttachments { get; set; }
        public string userId { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var account = await _userManager.FindByNameAsync(User?.Identity?.Name);
            var finalDocsAttachments = docsAttachments
               .Where(x => x.DocumentAttachment.Status == Status.Approved || x.DocumentAttachment.Status == Status.Disapproved)
               .ToList();
            if (User.IsInRole("Admin") || User.IsInRole("Reviewer"))
                DocsAttachments = finalDocsAttachments;
            else
                DocsAttachments = finalDocsAttachments.Where(x => x.SenderAccount.Id == account.Id).ToList();
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            userId = user?.Id;
        }

		public async Task<IActionResult> OnPostAddCommentAsync(string com, string id)
		{
			var documentAttachment = await _docRepo.GetOne(id);
			if (documentAttachment == null)
			{
				TempData["validation-message"] = "Document not found";
				return RedirectToPage();
			}

			documentAttachment.Comment = com;
			await _docRepo.Update(documentAttachment, documentAttachment.Id.ToString());
			TempData["validation-message"] = "Successfully added comment";
			return RedirectToPage();
		}

	}
}
