using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.RequestModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Report
{
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IDocumentAttachmentRepository _docsAttachRepo;
        private readonly IBaseRepository<SubCategory> _subCategRepo;
        private readonly IBaseRepository<Settings> _settingsRepo;
        public IndexModel(
            IDocumentAttachmentRepository docsAttachRepo,
            IBaseRepository<SubCategory> subCategRepo,
            IBaseRepository<Settings> settingsRepo)
        {
            _docsAttachRepo = docsAttachRepo;
            _subCategRepo = subCategRepo;
            _settingsRepo = settingsRepo;
        }
        public ReportsRecords Records { get; set; }
        public IEnumerable<SubCategory> SubCategories { get; set; }
        public int[] SelectItems { get; set; }
        public string Logo { get; set; }

        public async Task OnGetAsync(FilteringRequestModel request)
        {
            SubCategories = await _subCategRepo.GetAll();
            Records = await _docsAttachRepo.GetRecordsPiginated(request.SubCategory,request.Prioritization,request.Status);
            if (request.Status != null && request.Prioritization != null && request.SubCategory != null)
            {
                TempData["validation-message"] = "Successfully filtered the records";
            }
            var settings = await _settingsRepo.GetAll();
            Logo = settings.OrderByDescending(x => x.Id).First().LogoFileName;

        }
    }
}
