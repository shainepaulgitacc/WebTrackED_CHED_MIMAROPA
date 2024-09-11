using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Messenger
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class IndexModel : BasePageModel<Message, MessageInputModel>
    {
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
        private readonly IMessageRepository _messRepo;
        private readonly UserManager<AppIdentityUser> _userManager;
       

        public IndexModel(
            IBaseRepository<AppIdentityUser> userRepo,
            IMessageRepository messRepo,
            UserManager<AppIdentityUser> userManager,
            IMapper mapper
          
            ) :base(messRepo,mapper)
        {
            _userRepo = userRepo;
            _messRepo = messRepo;
            _userManager = userManager;
           
        }

        public List<AppIdentityUser> Users { get; set; }
        public AppIdentityUser Recipient { get; set; }
        public string? RecipientId { get; set; }
        public string? SenderId { get; set; }
        public List<MessengerData> Messages { get; set; }

        public AppIdentityUser UserAcc { get; set; }
        public List<Message> MessageRec { get; set; }

        public List<RecipientRecord> Recipients { get; set; }
        public async Task OnGetAsync(string? rId = null)
        {
           var users = await _userRepo.GetAll();
            var messages = await _messRepo.MessengerRecords();
            var messagesRecords = await _messRepo.GetAll();
            MessageRec = messagesRecords.ToList();
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            UserAcc = user;
            Recipients = messages
                .Where(x => x.Sender.Id == user.Id && x.Recipient.Id != user.Id)
                .GroupBy(x => x.Recipient.Id)
                .Select(s => new RecipientRecord
                {
                    Count = s.Count(),
                    Recipient = s.First().Recipient,
                    Message =  messagesRecords.Where(x => x.Recipient == s.Key && x.Sender == user.Id || x.Recipient == user.Id && x.Sender == s.Key).OrderByDescending(x => x.Id).First()
                })
                .OrderByDescending(x => x.Message.Id)
                .ToList();
            Users = users.Where(x => x.Id != user.Id).ToList();
            Messages = messages.ToList();
            RecipientId = rId?? string.Empty;
            SenderId = user?.Id;

            if(rId != null)
            {                
                Recipient = await _userManager.FindByIdAsync(rId);
                var messagesF = messagesRecords.Where(x => x.Sender == rId  && x.Recipient == user?.Id).ToList();
                foreach (var message in messagesF)
                {
                    message.IsViewed = true;
                    await _messRepo.Update(message, message.Id.ToString());
                }
            }

        }

        public async Task<JsonResult> OnGetRecipientsListSender()
        {
            var messages = await _messRepo.MessengerRecords();
            var messagesRecords = await _messRepo.GetAll();
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);

            
            var finResult = messages
               .Where(x => x.Sender.Id == user.Id && x.Recipient.Id != user.Id)
               .GroupBy(x => x.Recipient.Id)
               .Select(s => new
               {
                   Count = s.Count(),
                   RecipientId = s.First().Recipient.Id,
                   RecipientProfile = s.First().Recipient.ProfileFileName,
                   MessageLatestDate = messagesRecords.Where(x => x.Recipient == s.Key && x.Sender == user.Id || x.Recipient == user.Id && x.Sender == s.Key).OrderByDescending(x => x.Id).First().AddedAt,
                   RecipientFullName = $"{s.First().Recipient.FirstName} {s.First().Recipient.MiddleName} {s.First().Recipient.LastName} {s.First().Recipient.Suffixes}",
                   HasUnviewed = messagesRecords.Any(x => x.Recipient == user.Id && x.Sender == s.Key && !x.IsViewed)
               })
               .OrderByDescending(x => x.MessageLatestDate)
               .ToList();
           
            return new JsonResult(finResult);
        }
        public async Task<JsonResult> OnGetRecipientsListRecipient(string recipientId)
        {
            var messages = await _messRepo.MessengerRecords();
            var messagesRecords = await _messRepo.GetAll();
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);


            var finResult = messages
               .Where(x => x.Sender.Id == recipientId && x.Recipient.Id != recipientId)
               .GroupBy(x => x.Recipient.Id)
               .Select(s => new
               {
                   Count = s.Count(),
                   RecipientId = s.First().Recipient.Id,
                   RecipientProfile = s.First().Recipient.ProfileFileName,
                   MessageLatestDate = messagesRecords.Where(x => x.Recipient == s.Key && x.Sender == recipientId || x.Recipient == recipientId && x.Sender == s.Key).OrderByDescending(x => x.Id).First().AddedAt,
                   RecipientFullName = $"{s.First().Recipient.FirstName} {s.First().Recipient.MiddleName} {s.First().Recipient.LastName} {s.First().Recipient.Suffixes}",
                   HasUnviewed = messagesRecords.Any(x => x.Recipient == user.Id && x.Sender == s.Key && !x.IsViewed)
               })
               .OrderByDescending(x => x.MessageLatestDate)
               .ToList();
            return new JsonResult(finResult);
        }
    }
}
