using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Configuration;
using System.Runtime.CompilerServices;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Management.OfficeManagement
{
	[Authorize(Roles = "Admin")]
	public class IndexModel:BasePageModel<Office,OfficeInputModel>
    {
        private IBaseRepository<Office> _repo;
        public IndexModel(IBaseRepository<Office> officeRepo,IMapper map):base(officeRepo,map)
        {
            _repo = officeRepo;
        }
        public async Task OnGetAsync()
        {
            var offices = await _repo.GetAll();
            Records = offices.ToList();
        }
        public override async Task<IActionResult> OnPostAsync(string? pageName = null, string? pId = null, bool hasMessage = true)
        {
            var offices = await _repo.GetAll();
            if(offices.Any(x => x.OfficeName == InputModel.OfficeName))
            {
                TempData["validation-message"] = "Office name is already existing";
                return RedirectToPage();
            }
                
            await OnPostAsync(pageName, pId, hasMessage);
            return RedirectToPage();
        }
        public async override Task<IActionResult> OnGetDelete(string Id, string? returnUrl = null, string? pId = null, bool hasMessage = true)
        {
            var offices = await _repo.GetAll();
            if(offices.First().Id == int.Parse(Id))
            {
                TempData["validation-message"] = "Can't delete the first record";
                return RedirectToPage();
            }
            await base.OnGetDelete(Id);
            return RedirectToPage();
        }

        }
}
