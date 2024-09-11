using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Management.SubCategoryManagement
{
    [Authorize(Roles="Admin")]
    public class IndexModel : BasePageModel<SubCategory, SubCategoryInputModel>
    {
        public IBaseRepository<SubCategory> _repo { get; set; }
        public IBaseRepository<Category> _categRepo { get; set; }
        public IMapper _mapper { get; set; }
        public IndexModel(IBaseRepository<SubCategory> repo,
                          IMapper mapper,
                          IBaseRepository<Category> categRepo):base(repo,mapper)
        {
            _repo = repo;
            _mapper = mapper;
            _categRepo = categRepo;
        }
        public List<Category> Categories { get; set; }
        public async Task OnGetAsync()
        {
            var records = await _repo.GetAll();
            var categories = await _categRepo.GetAll();
            Categories = categories.ToList();
            Records = records.ToList();
        }
    }
}
