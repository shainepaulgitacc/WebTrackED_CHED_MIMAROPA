using Microsoft.AspNetCore.SignalR;
using WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts;

namespace WebTrackED_CHED_MIMAROPA.Hubs
{
    public class NotificationHub: Hub<INotificationHub>
    {
        public async Task SendNotification(string title, string description, string notifType, string date)
        {
        }
    }
}
