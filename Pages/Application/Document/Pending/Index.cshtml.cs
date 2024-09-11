using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.Pending
{
    [Authorize(Roles = "Sender")]
    public class IndexModel : BasePageModel<DocumentAttachment, DocumentAttachment>
    {
        private readonly IDocumentAttachmentRepository _docAttRepo;
        private readonly IBaseRepository<Sender> _senderRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
        private readonly IMapper _mapper;
        public IndexModel(
            IDocumentAttachmentRepository docAttRepo,
            UserManager<AppIdentityUser> userManager,
            IBaseRepository<Sender> senderRepo,
            IMapper mapper):base(docAttRepo,mapper)
        {
            _docAttRepo = docAttRepo;
            _senderRepo = senderRepo;
            _userManager = userManager;
            _mapper = mapper;
        }
        public List<DocumentAttachmentViewModel> docAttachments { get; set; }
        public async Task OnGetAsync() 
        {
           
            var senders = await _senderRepo.GetAll();
            var user = await _userManager.FindByNameAsync(User?.Identity?.Name);
            var senderId = senders.FirstOrDefault(x => x.IdentityUserId == user?.Id)?.Id;
            var docAttachments = await _docAttRepo.DocumentAttachments();
            this.docAttachments = docAttachments.Where(x => x.SenderAccount.Id == user.Id && x.DocumentAttachment.Status == Status.Pending).ToList();
        }
        public async override Task<IActionResult> OnGetDelete(string Id, string? returnUrl = null, string? pId = null, bool hasMessage = true)
        {
            var document = await _docAttRepo.GetOne(Id);
            var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Documents", document.FileName);
            if (System.IO.File.Exists(documentPath))
                System.IO.File.Delete(documentPath);
            await base.OnGetDelete(Id);
            return RedirectToPage();
        }
    }
}
