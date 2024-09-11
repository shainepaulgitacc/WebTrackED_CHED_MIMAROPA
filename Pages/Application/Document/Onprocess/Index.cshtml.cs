using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.OnProcess
{

	[Authorize(Roles = "Sender")]
	public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly FileUploader _fileUploader;
        private readonly IMapper _mapper;
        public IndexModel(
            IDocumentAttachmentRepository docRepo,
            IBaseRepository<Sender> senderRepo,
            UserManager<AppIdentityUser> userManager,
            FileUploader fileUploader,
            IMapper mapper)
        {
            _docRepo = docRepo;
            _senderRepo = senderRepo;
            _userManager = userManager;
            _fileUploader = fileUploader;
            _mapper = mapper;
        }

        public List<DocumentAttachmentViewModel> docAttachments { get; set; }
        public async Task OnGetAsync()
        {
            var docsAttachments = await _docRepo.DocumentAttachments();
            var account = await _userManager.FindByEmailAsync(User?.Identity?.Name);
          
            var filterRecords = docsAttachments.OrderByDescending(x => x.DocumentTracking.Id).ToList();
            docAttachments = docsAttachments
               .Where(x => x.DocumentAttachment.SenderId == account.Id && x.DocumentAttachment.Status == Status.OnProcess || x.DocumentAttachment.Status == Status.PreparingRelease)
               .ToList();
        }
        public async Task<IActionResult> OnGetDownloadFile(string filename)
        {
            try
            {
                byte[] fileBytes = await _fileUploader.DownloadFile(filename);
                return File(fileBytes, "application/octet-stream", filename);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
