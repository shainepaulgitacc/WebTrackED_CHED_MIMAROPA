using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.RequestModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.Application.NotificationManagement
{
	[Authorize]
	[ValidateAntiForgeryToken]
	public class IndexModel:PageModel
	{
		private readonly IBaseRepository<Notification> _notificationRepo;
		private readonly UserManager<AppIdentityUser> _userManager;

		public IndexModel(
			IBaseRepository<Notification> notificationRepo,
			UserManager<AppIdentityUser> userManager,
			IMapper mapper)
		{
			_notificationRepo = notificationRepo;
			_userManager = userManager;
		}

		public List<Notification> Notifications { get; set; }

		public DateTime? From { get; set; }
		public DateTime? To { get; set; }
		public async Task OnGetAsync([FromQuery] DateTime? fNotif_From, [FromQuery] DateTime? fNotif_To)
		{
			var notifications = await _notificationRepo.GetAll();
			var user = await _userManager.FindByNameAsync(User.Identity.Name);

			From = fNotif_From;
			To = fNotif_To;

			if (From != null && To != null)
			{
				Notifications = notifications.Where(x => x.Recepient == user.Id && !x.IsArchived && x.AddedAt.Date >= From.Value.Date && x.AddedAt.Date <= To.Value.Date).ToList();
			}
			else
			{
				Notifications = notifications.Where(x => x.Recepient == user.Id && !x.IsArchived).ToList();
			}
		}

		public async Task<IActionResult> OnGetGetAll()
		{
			TempData["validation-message"] = "Successfully retrieved all notifications.";
			return RedirectToPage();
		}
		public async Task<IActionResult> OnGetAction(string Id, NotifActionType t, string? From = null, string? To = null)
		{
			var notification = await _notificationRepo.GetOne(Id);
			if (notification == null)
			{
				return BadRequest($"Invalid Id");
			}
			if (t == NotifActionType.View)
			{
				notification.IsViewed = true;
			}
			if (t == NotifActionType.Delete)
			{
				await _notificationRepo.Delete(Id);
				TempData["validation-message"] = "Successfully deleted";
				return RedirectToPage("./Index", new { fNotif_From = From, fNotif_To = To });
			}
			if (t == NotifActionType.Archived)
			{
				notification.IsArchived = true;
				TempData["validation-message"] = "Successfully archived notification";
			}
			if (t == NotifActionType.Restore)
			{
				notification.IsArchived = false;
				TempData["validation-message"] = "Successfully restore notification";
			}
			notification.UpdatedAt = DateTime.UtcNow.AddHours(8) 
				;
			await _notificationRepo.Update(notification, notification.Id.ToString());
			return RedirectToPage("./Index", new { fNotif_From = From, fNotif_To = To });
		}
		
	}
}
