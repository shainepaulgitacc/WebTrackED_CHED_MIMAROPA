using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application
{
    [ValidateAntiForgeryToken]
    public class BasePageModel<T, T2> : PageModel
        where T : BaseEntity
        where T2 : class
    {
        private IBaseRepository<T> _repo { get; set; }
        private IMapper _mapper { get; set; }
        public BasePageModel(IBaseRepository<T> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [BindProperty]
        public T2 InputModel { get; set; }

        public List<T> Records { get; set; }
        public async Task<IActionResult> OnPostAsync(string? pageName = null,string? pId= null,bool  hasMessage = true)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            var converted = _mapper.Map<T>(InputModel);
            converted.AddedAt = DateTime.Now;
            converted.UpdatedAt = DateTime.Now;
            await _repo.Add(converted);
            TempData["validation-message"] = hasMessage ? "Successfully added":null;
            if (pId == null && pageName != null)
                return RedirectToPage(pageName);
            if (pId != null && pageName != null)
                return RedirectToPage(pageName, new {pId});
            return RedirectToPage();
        }

       
        public async Task<IActionResult> OnPostUpdate()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var converted = _mapper.Map<T>(InputModel);
            converted.UpdatedAt = DateTime.Now;
            await _repo.Update(converted, converted.Id.ToString());
            TempData["validation-message"] = "Successfully updated";
            return RedirectToPage();
        }
        public async virtual Task<IActionResult> OnGetDelete(string Id,string? returnUrl = null, string? pId = null, bool hasMessage = true)
        {
            if (Id == null)
                return BadRequest("Id is Null");
            try
            {
                await _repo.Delete(Id);
                TempData["validation-message"] = hasMessage ? "Successfully deleted" : null;
                if (pId == null && returnUrl != null)
                    return RedirectToPage(returnUrl);
                if (pId != null && returnUrl != null)
                    return RedirectToPage(returnUrl, new { pId });
                return RedirectToPage();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlException && (sqlException.Number == 547 || sqlException.Number == 515))
                {
                    TempData["validation-message"] = "Delete the child data first";
                    if (returnUrl != null)
                        return RedirectToPage(returnUrl);
                    return RedirectToPage();
                }
                else
                {
                    TempData["validation-message"] = "Unknown error";
                    if (returnUrl != null)
                        return RedirectToPage(returnUrl);
                    return RedirectToPage();
                }
            }
        }
    }
}
