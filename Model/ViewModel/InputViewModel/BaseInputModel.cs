namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class BaseInputModel
    {
        public int? Id { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
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
