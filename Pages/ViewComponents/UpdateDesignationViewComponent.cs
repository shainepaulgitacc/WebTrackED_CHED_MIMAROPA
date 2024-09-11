using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.ViewComponents
{
    public class UpdateDesignationViewComponent:ViewComponent
    {
        private readonly IBaseRepository<Designation> _repo;
        private readonly IMapper _mapper;
        public UpdateDesignationViewComponent(IBaseRepository<Designation> repo,IMapper mapper)
        {
            _repo = repo;  
            _mapper = mapper;
        }
        public async Task<IViewComponentResult> InvokeAsync(string Id)
        {
            var designation = await _repo.GetOne(Id);
            var converted = _mapper.Map<DesignationInputModel>(designation);
            return View(converted);
        }
    }
}
