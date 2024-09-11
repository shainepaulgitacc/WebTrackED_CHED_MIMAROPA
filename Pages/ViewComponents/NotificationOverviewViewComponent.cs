using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ViewComponentModel;

namespace WebTrackED_CHED_MIMAROPA.Pages.ViewComponents
{
	public class NotificationOverviewViewComponent:ViewComponent
	{
		private readonly IBaseRepository<Notification> _repo;
		private readonly UserManager<AppIdentityUser> _userManager;

		public NotificationOverviewViewComponent(
			IBaseRepository<Notification> repo,
			UserManager<AppIdentityUser> userManager)
		{
			_repo = repo;
			_userManager = userManager;
		}

		public async Task<IViewComponentResult> InvokeAsync(bool s)
		{
			var notifications = await _repo.GetAll();
			var user = await _userManager.FindByNameAsync(User.Identity?.Name);
			var filtered = notifications.Where(x => x.Recepient == user.Id).ToList();
			return View(new NotificationOverviewViewModel
			{
				Notifications = filtered,
				IsPageArchived = s
			});
		}
	}
}
