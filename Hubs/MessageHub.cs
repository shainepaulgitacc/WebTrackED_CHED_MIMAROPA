using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using WebTrackED_CHED_MIMAROPA.Data;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

public class MessageHub : Hub<IMessageHub>
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IMessageRepository _messRepo;

    public MessageHub(ApplicationDbContext db, UserManager<AppIdentityUser> userManager, IMessageRepository messRepo)
    {
        _db = db;
        _userManager = userManager;
        _messRepo = messRepo;
    }

    private async Task AddMessage(string senderId, string recipientId, string messageContent)
    {
        try
        {
            await _db.Messages.AddAsync(new Message
            {
                Sender = senderId,
                Recipient = recipientId,
                MessageContent = messageContent,
                UpdatedAt = DateTime.UtcNow.AddHours(8),
                AddedAt = DateTime.UtcNow.AddHours(8)
            });

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error adding message: {ex.Message}");
            throw;
        }
    }


    private async Task<List<RecipientRecord>> RetreivedRecpients(string recipientId)
    {
        var messages = _db.Messages.ToList();
        var messageJoined = await _messRepo.MessengerRecords();
        return messageJoined
                .Where(x => x.Sender.Id == recipientId && x.Recipient.Id != recipientId)
                .GroupBy(x => x.Recipient.Id)
                .Select(s => new RecipientRecord
                {
                    Count = s.Count(),
                    Recipient = s.First().Recipient,
                    Message = messages.Where(x => x.Recipient == s.Key && x.Sender == recipientId || x.Recipient == recipientId && x.Sender == s.Key).OrderByDescending(x => x.Id).First()
                })
                .OrderByDescending(x => x.Message.Id)
                .ToList();
    }

    private async Task Delete(int messageId)
    {
        try
        {
            var message = await _db.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.IsDeleted = true;
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting message: {ex.Message}");
            throw;
        }
    }

    public async Task SendMessage(string senderId, string recipientId, string messageContent)
    {
        try
        {
            await AddMessage(senderId, recipientId, messageContent);
            var sender = await _userManager.FindByIdAsync(senderId);

            var messages = _db.Messages.ToList();
            var countMessage = messages.Count(x => x.Recipient == recipientId && !x.IsViewed);
            var finalValue = countMessage > 10 ? $"{countMessage}+" : countMessage.ToString();

            var finRecipients = await RetreivedRecpients(recipientId);

            var newMessageId = messages.OrderByDescending(x => x.Id).First().Id;
            await Clients.User(recipientId).CountMessage(finalValue);
            await Clients.User(recipientId).ReceiveMessage(senderId, recipientId, messageContent, sender.ProfileFileName,newMessageId);
            await Clients.User(senderId).Sender(newMessageId);

            await Clients.User(recipientId).Recipients(recipientId);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending message: {ex.Message}");
            throw;
        }
    }

    public async Task RecipientsList(string recipientId)
    {
        try
        {
            var messageJoined = await _messRepo.MessengerRecords();
            var messages = _db.Messages.ToList();
            var finRecipients = messageJoined
                .Where(x => x.Sender.Id == recipientId && x.Recipient.Id != recipientId)
                .GroupBy(x => x.Recipient.Id)
                .Select(s => new RecipientRecord
                {
                    Count = s.Count(),
                    Recipient = s.First().Recipient,
                    Message = messages.Where(x => x.Recipient == s.Key && x.Sender == recipientId || x.Recipient == recipientId && x.Sender == s.Key).OrderByDescending(x => x.Id).First()
                })
                .OrderByDescending(x => x.Message.Id)
                .ToList();

           
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching recipients list: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteMessage(string messageId, int index)
    {
        try
        {
            var message = await _db.Messages.FindAsync(int.Parse(messageId));
            if (message != null)
            {
                await Delete(int.Parse(messageId));
                await Clients.User(message.Recipient).RemoveMessage(message.Sender,int.Parse(messageId));
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting message: {ex.Message}");
            throw;
        }
    }
}
