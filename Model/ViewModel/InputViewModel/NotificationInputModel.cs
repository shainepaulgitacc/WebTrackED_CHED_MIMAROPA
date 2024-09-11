using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class NotificationInputModel:BaseInputModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsViewed { get; set; }
        public string? Recepient { get; set; }

        public bool IsArchived { get; set; }
        public string? RedirectLink { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}
