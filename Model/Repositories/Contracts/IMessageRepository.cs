using WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
    public interface IMessageRepository:IBaseRepository<Message>
    {
        Task<IEnumerable<MessengerData>> MessengerRecords();
    }
}
