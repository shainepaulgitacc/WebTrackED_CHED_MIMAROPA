using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Linq;
using System.Xaml.Permissions;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document
{

	[Authorize]
	public class DocumentTrackingModel : PageModel
    {
        private readonly IDocumentTrackingRepository _docTrackRepo;
        private readonly IDocumentAttachmentRepository _docAttachmentRepo;
     

        private readonly UserManager<AppIdentityUser> _userManager;
        public DocumentTrackingModel(
            IDocumentTrackingRepository docTrackRepo,
            IDocumentAttachmentRepository docAttachmentRepo,
          
            UserManager<AppIdentityUser> userManager)
        {
            _docTrackRepo = docTrackRepo;
            _docAttachmentRepo = docAttachmentRepo;
          
            _userManager = userManager;
        }
        public string PreviousPage { get; set; }
        public List<DocumentTrackingViewModel> DocumentTrackings { get; set; }
     
        public DocumentAttachmentViewModel DocumentAttachment { get; set; }

        public string UserId { get; set; }
        public int MaxId { get; set; }


        public bool IsPending { get; set; }
		
        public bool IsOnProcess { get; set; }
		public bool IsApproved { get; set; }
        public bool IsPreparingForRelease { get; set; }
        public bool IsCompleted { get; set; }
		public async Task<IActionResult> OnGetAsync(string prevPage, int pId)
		{
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var docsAttachment = await _docAttachmentRepo.DocumentAttachments();
            var document = docsAttachment.FirstOrDefault(x => x.DocumentAttachment.Id == pId);

			UserId = user.Id;
			PreviousPage = prevPage;

			var docsTrackings = await _docTrackRepo.DocumentTrackings();

		
			var filteredDocsTracking = docsTrackings
				.Where(x => x.DocumentAttachment.Id == pId)
				.OrderByDescending(x => x.DocumentTracking.Id)
				.ToList();

			if(User.IsInRole("Sender")&& document.SenderAccount.Id != user.Id)
                return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });

            if (!filteredDocsTracking.Any())
				return BadRequest("Document doesn't exist");

			DocumentTrackings = filteredDocsTracking;
			DocumentAttachment = document;

			// Set status flags
			IsPending = !document.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.OnReview); ;
			IsOnProcess = document.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.OnReview) && !document.DocumentTrackings.Any(x => x.ReviewerStatus == ReviewerStatus.Completed || x.ReviewerStatus == ReviewerStatus.PreparingRelease || x.ReviewerStatus == ReviewerStatus.Approved);
			IsApproved = document.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.Approved;
            IsPreparingForRelease =document.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.PreparingRelease;
			IsCompleted = document.DocumentTrackings.First().ReviewerStatus == ReviewerStatus.Completed;

			return Page();
		}

	}
}
	