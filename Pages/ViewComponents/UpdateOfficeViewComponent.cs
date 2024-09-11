using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.ViewComponents
{
    public class UpdateOfficeViewComponent:ViewComponent
    {
        public IBaseRepository<Office> _repo;
        public IMapper _mapper;
        public UpdateOfficeViewComponent(IBaseRepository<Office> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync(string Id)
        {
            var office = await _repo.GetOne(Id);
            var converted = _mapper.Map<OfficeInputModel>(office);
            return View(converted);
        }
    }
}
