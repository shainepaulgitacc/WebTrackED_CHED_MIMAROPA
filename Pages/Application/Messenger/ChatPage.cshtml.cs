using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.Messenger
{
    [Authorize]
    [ValidateAntiForgeryToken]
    public class ChatPageModel : PageModel
    {
        private readonly IBaseRepository<AppIdentityUser> _userRepo;
        private readonly IMessageRepository _messRepo;
        private readonly UserManager<AppIdentityUser> _userManager;


        public ChatPageModel(
            IBaseRepository<AppIdentityUser> userRepo,
            IMessageRepository messRepo,
            UserManager<AppIdentityUser> userManager,
            IMapper mapper

            )
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

        public IEnumerable<Message> MessagesRec { get; set; }
        public AppIdentityUser UserAcc { get; set; }

        public List<RecipientRecord> Recipients { get; set; }
        public async Task OnGetAsync(string? rId = null)
        {
            var users = await _userRepo.GetAll();
            MessagesRec = await _messRepo.GetAll();

            var messages = await _messRepo.MessengerRecords();
            var messagesRecords = await _messRepo.GetAll();
            var user = await _userManager.FindByNameAsync(User.Identity?.Name);
            UserAcc = user;
            Recipients = messages
                .Where(x => x.Sender.Id == user.Id && x.Recipient.Id != user.Id)
                .GroupBy(x => x.Recipient.Id)
                .Select(s => new RecipientRecord
                {
                    Count = s.Count(),
                    Recipient = s.First().Recipient,
                    Message = s.First().Message,
                })
                .OrderByDescending(x => x.Message.Id)
                .ToList();
            Users = users.Where(x => !messages.Any(s => s.Sender.Id == user.Id && s.Recipient.Id == x.Id) && x.Id != user.Id).ToList();
            Messages = messages.ToList();
            RecipientId = rId ?? string.Empty;
            SenderId = user?.Id;

            if (rId != null)
            {
                Recipient = await _userManager.FindByIdAsync(rId);
                var messagesF = messagesRecords.Where(x => x.Sender == rId && x.Recipient == user?.Id).ToList();
                foreach (var message in messagesF)
                {
                    message.IsViewed = true;
                    await _messRepo.Update(message, message.Id.ToString());
                }
            }

        }
    }
}
