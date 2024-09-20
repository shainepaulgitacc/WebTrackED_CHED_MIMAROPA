using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.Ended
{
    [Authorize(Roles = "Sender")]
    public class WalkInDocumentModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        public WalkInDocumentModel(IDocumentAttachmentRepository docRepo)
        {
            _docRepo = docRepo;
        }
        public string RefCode;
        public bool IsWalkIn;
        public async Task OnGetAsync(string? RefCode = null)
        {
            if (!string.IsNullOrEmpty(RefCode))
            {
                var id = new string(RefCode?.Where(char.IsDigit).ToArray());
                var document = await _docRepo.GetOne(id);

                this.RefCode = id;
                IsWalkIn = document != null && document.DocumentType == Model.Entities.DocumentType.WalkIn;
            }
          
        }
        
      
    }
}
