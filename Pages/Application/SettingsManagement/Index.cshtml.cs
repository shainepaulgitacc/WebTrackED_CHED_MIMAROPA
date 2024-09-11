using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.Service;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.SettingsManagement
{
	[Authorize(Roles ="Admin")]
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IBaseRepository<Settings> _settingsRepo;
        private readonly FileUploader _fileUploader;
        private readonly IMapper _mapper;

        public IndexModel(IBaseRepository<Settings> settingsRepo,IMapper mapper, FileUploader fileUploader)
        {
            _settingsRepo = settingsRepo;
            _fileUploader = fileUploader;
            _mapper = mapper;
        }

        public SettingsInputModel SettingsInput { get; set; }
        public ChangeLogoInputModel ChangeLogoInput { get; set; }
        public async Task OnGetAsync()
        {
            var settingsRecords = await _settingsRepo.GetAll();
            var settingsUpdated = settingsRecords.OrderByDescending(x => x.Id).First();
            SettingsInput = _mapper.Map<SettingsInputModel>(settingsUpdated);
            
        }

        public async Task<IActionResult> OnPostAsync(SettingsInputModel SettingsInput)
        {
            this.SettingsInput = SettingsInput;
            if (!TryValidateModel(this.SettingsInput))
                return BadRequest(ModelState);
            var converted = _mapper.Map<Settings>(this.SettingsInput);
            await _settingsRepo.Update(converted,converted.Id.ToString());
			TempData["validation-message"] = "Successfully save changes";
            return RedirectToPage();
		}

        public async Task<IActionResult> OnPostChangeLogo(ChangeLogoInputModel ChangeLogoInput,string Id)
        {
            this.ChangeLogoInput = ChangeLogoInput;
            if(!TryValidateModel(this.ChangeLogoInput))
                return BadRequest(ModelState);
            var settings = await _settingsRepo.GetOne(Id);
            settings.LogoFileName = await _fileUploader.Uploadfile(this.ChangeLogoInput.LogoFile,"Logo");
            await _settingsRepo.Update(settings, settings.Id.ToString());
            TempData["validation-message"] = "Successfully change logo";
            return RedirectToPage();
            
		}
    }
}
