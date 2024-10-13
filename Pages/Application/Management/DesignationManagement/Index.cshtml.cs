using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Management.DesignationManagement
{
	[Authorize(Roles = "Admin")]
	public class IndexModel : BasePageModel<Designation,DesignationInputModel>
    {
        private IBaseRepository<Designation> _repo;
       
        public IndexModel(IBaseRepository<Designation> repo, 
                         
                          IMapper map) : base(repo, map)
        {
            _repo = repo;
          
        }
     
        public async Task OnGetAsync()
        {
            var designations = await _repo.GetAll();
         
            Records = designations.ToList();
          
        }

        public override async Task<IActionResult> OnPostAsync(string? pageName = null, string? pId = null, bool hasMessage = true)
        {
            var designations = await _repo.GetAll();
          
            if(designations.Any(x => x.DesignationName.Contains(InputModel.DesignationName)))
            {
                TempData["validation-message"] = "Already existing designation name";
                return RedirectToPage();
            }
            await base.OnPostAsync(pageName, pId, hasMessage);
            return RedirectToPage();
        }
        public override async Task<IActionResult> OnGetDelete(string Id, string? returnUrl = null, string? pId = null, bool hasMessage = true)
        {
            var designations = await _repo.GetAll();
            var firstDesignation = designations.OrderBy(x => x.AddedAt).First();
            var secondDesignation = designations.OrderBy(x => x.AddedAt).Skip(1).First();
            if (firstDesignation.Id == int.Parse(Id) || secondDesignation.Id == int.Parse(Id))
            {
                TempData["validation-message"] = "Can't delete this designation record";
                return RedirectToPage();
            }
            await base.OnGetDelete(Id, returnUrl, pId, hasMessage);
            return RedirectToPage();
        }
    }
}
