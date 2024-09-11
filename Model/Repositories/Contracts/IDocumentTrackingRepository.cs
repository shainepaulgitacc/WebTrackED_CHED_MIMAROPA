
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
    public interface IDocumentTrackingRepository:IBaseRepository<DocumentTracking>
    {
        Task<IEnumerable<DocumentTrackingViewModel>> DocumentTrackings();
    }
}
