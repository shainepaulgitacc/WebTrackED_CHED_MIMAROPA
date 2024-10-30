namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class BaseInputModel
    {
        public int? Id { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow.AddHours(8);
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(8);
    }

    public enum ExportAsPdfExcel
    {
        Excel,
        Pdf
    }

    public enum NotifActionType
    {
        View,
        Delete,
        Archived,
        Restore
    }
}
