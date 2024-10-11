using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class ReportsRecords
    {
        public IEnumerable<DocumentAttachmentViewModel> Records { get; set; }
        public int? ServiceId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
