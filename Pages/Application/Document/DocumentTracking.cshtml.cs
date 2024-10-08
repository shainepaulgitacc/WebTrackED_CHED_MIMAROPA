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
        public bool IsPreparingForRelease { get; set; }
        public bool IsCompleted { get; set; }
		public async Task<IActionResult> OnGetAsync(string prevPage, int pId)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			var documentAtt = await _docAttachmentRepo.GetOne(pId.ToString());

			// Determine if the document is "WalkIn" (skip the first record for walk-ins, otherwise no skip)
			var skipValue = documentAtt.DocumentType == DocumentType.WalkIn ? 1 : 0;

			UserId = user.Id;
			PreviousPage = prevPage;

			var docsTrackings = await _docTrackRepo.DocumentTrackings();

			// Apply filtering and conditional skipping
			var filteredDocsTracking = docsTrackings
				.Where(x => x.DocumentAttachment.Id == pId)
				//.Skip(skipValue) // Skip if it's a WalkIn document, otherwise no skip
				.OrderByDescending(x => x.DocumentTracking.Id)
				.ToList();

			if (!filteredDocsTracking.Any())
				return BadRequest("Document doesn't exist");

			DocumentTrackings = filteredDocsTracking;

			var mxId = filteredDocsTracking.Max(x => x.DocumentTracking.Id);
			MaxId = mxId;

			var docsAttachment = await _docAttachmentRepo.DocumentAttachments();
			var document = docsAttachment.FirstOrDefault(x => x.DocumentTracking.Id == mxId);

			DocumentAttachment = document;

			// Set status flags
			IsPending = document.DocumentAttachment.Status == Status.Pending;
			IsOnProcess = document.DocumentAttachment.Status == Status.OnProcess;
			IsPreparingForRelease = document.DocumentAttachment.Status == Status.PreparingRelease;
			IsCompleted = document.DocumentAttachment.Status == Status.Disapproved || document.DocumentAttachment.Status == Status.Approved;

			return Page();
		}

	}
}
