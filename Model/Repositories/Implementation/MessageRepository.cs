using System.Security.Cryptography.Pkcs;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Implementation
{
    public class MessageRepository : BaseRepository<Message>, IMessageRepository
    {
        private readonly ApplicationDbContext _db;
        public MessageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<MessengerData>> MessengerRecords()
        {
            var recipients = _db.Set<AppIdentityUser>().ToList();
            var messages = _db.Set<Message>().ToList();
            var senders = _db.Set<AppIdentityUser>().ToList();
            return messages
                .Join(recipients,
                mess => mess.Recipient,
                user => user.Id,
                (mess, recipt) => new
                {
                    Message = mess,
                    Recipient = recipt
                })
                .Join(senders ,
                result => result.Message.Sender,
                sender => sender.Id,
                (result ,sender) => new MessengerData
                {
                    Recipient = result.Recipient,
                    Sender = sender,
                    Message = result.Message
                })
                .ToList();
        }
    }
}
