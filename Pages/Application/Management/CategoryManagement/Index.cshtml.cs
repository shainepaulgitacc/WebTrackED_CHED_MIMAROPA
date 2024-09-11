using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Management.CategoryManagement
{
    [Authorize(Roles ="Admin")]
    public class IndexModel : BasePageModel<Category, CategoryInputModel>
    {
        private IBaseRepository<Category> _repo;
        private IMapper _mapper { get; set; }
        public IndexModel(IBaseRepository<Category> repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
        }
        public async Task OnGetAsync()
        {
            var records = await _repo.GetAll();
            Records = records.ToList();
        }
    }
}
