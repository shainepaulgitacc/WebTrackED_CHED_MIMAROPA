using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Document.WalkIn
{
    [Authorize(Roles = "Sender")]
    public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docRepo;
        public IndexModel(IDocumentAttachmentRepository docRepo)
        {
            _docRepo = docRepo;
        }
        public string RefCode;
        public bool IsWalkIn;
        public async Task OnGetAsync(string? refCode = null)
        {
            if (!string.IsNullOrEmpty(refCode))
            {
                var id = new string(refCode?.Where(char.IsDigit).ToArray());
                
                if(int.TryParse(id, out int parsed))
                {
                    var document = await _docRepo.GetOne(id);

                   
                    IsWalkIn = document != null && document.DocumentType == Model.Entities.DocumentType.WalkIn;
                    RefCode = id;
                }
                else
                {
                    IsWalkIn = false;
                    RefCode = refCode;
                }
               
            }

        }


    }
}
