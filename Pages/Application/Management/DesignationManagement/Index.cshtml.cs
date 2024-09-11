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
        private IBaseRepository<Office> _officeRepo;
        public IndexModel(IBaseRepository<Designation> repo, 
                          IBaseRepository<Office> officeRepo,
                          IMapper map) : base(repo, map)
        {
            _repo = repo;
            _officeRepo = officeRepo;
        }
        public List<Office> Offices { get; set; }
        public async Task OnGetAsync()
        {
            var designations = await _repo.GetAll();
            var offices = await _officeRepo.GetAll();
            Records = designations.ToList();
            Offices = offices.ToList(); 
        }
    }
}
