namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
    public interface INotificationHub
    {
        Task ReceiveNotification(string title, string description, string notifType, string date,string redirectLink);

        Task ReviewerRealtime();
    }
}
