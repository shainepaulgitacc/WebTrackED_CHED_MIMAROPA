using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
    public interface IDocumentAttachmentRepository:IBaseRepository<DocumentAttachment>
    {
        Task<IEnumerable<DocumentAttachmentViewModel>> DocumentAttachments();
		
		Task<ReportsRecords> GetRecordsPiginated(int? serviceId,DateTime? from, DateTime? to);
    }
}
