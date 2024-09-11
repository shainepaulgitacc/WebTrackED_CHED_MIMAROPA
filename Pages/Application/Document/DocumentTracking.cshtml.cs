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
        private readonly IBaseRepository<DocumentProcedure> _docProcedureRepo;

        private readonly UserManager<AppIdentityUser> _userManager;
        public DocumentTrackingModel(
            IDocumentTrackingRepository docTrackRepo,
            IDocumentAttachmentRepository docAttachmentRepo,
            IBaseRepository<DocumentProcedure> docProcedureRepo,
            UserManager<AppIdentityUser> userManager)
        {
            _docTrackRepo = docTrackRepo;
            _docAttachmentRepo = docAttachmentRepo;
            _docProcedureRepo = docProcedureRepo;
            _userManager = userManager;
        }
        public string PreviousPage { get; set; }
        public List<DocumentTrackingViewModel> DocumentTrackings { get; set; }
        public List<DocumentProcedure> DocumentProcedures { get; set; }
        public DocumentAttachmentViewModel DocumentAttachment { get; set; }

        public string UserId { get; set; }
        public int MaxId { get; set; }
        public async Task<IActionResult> OnGetAsync(string prevPage, int pId)
        {
            var documentProcedures = await _docProcedureRepo.GetAll();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            UserId = user.Id;

            DocumentProcedures = documentProcedures
                .Where(x => x.DocumentAttachmentId == pId)
                .ToList();
            PreviousPage = prevPage;
            var docsTrackings = await _docTrackRepo
                .DocumentTrackings();
            var filteredDocsTracking = docsTrackings
                .Where(x => x.DocumentAttachment.Id == pId)
                .OrderByDescending(x => x.DocumentTracking.Id)
                .ToList();
            if (docsTrackings.Count() == 0)
                return BadRequest("Document don't exist");
            DocumentTrackings = filteredDocsTracking;
            var mxId = filteredDocsTracking.Max(x => x.DocumentTracking.Id);
            MaxId = mxId;
            var docsAttachment = await _docAttachmentRepo.DocumentAttachments();
            DocumentAttachment = docsAttachment
                .FirstOrDefault(x =>x.DocumentTracking.Id == mxId);
            return Page();
        }
    }
}
