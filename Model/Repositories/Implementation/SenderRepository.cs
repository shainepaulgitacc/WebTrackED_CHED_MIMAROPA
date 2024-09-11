using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class SenderRepository:BaseRepository<Sender>,ISenderRepository
    {
        private readonly ApplicationDbContext _db;
        public SenderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SenderListViewModel>> SenderRecords()
        {
            var senders = _db.Set<Sender>().ToList();
            var senderAccounts = _db.Set<AppIdentityUser>().ToList();
            return senders
                .Join(senderAccounts,
                sender => sender.IdentityUserId,
                sAcc => sAcc.Id,
                (sender, sAcc) => new SenderListViewModel
                {
                    Sender = sender,
                    User = sAcc,
                })
                .ToList();
        }
    }
}
