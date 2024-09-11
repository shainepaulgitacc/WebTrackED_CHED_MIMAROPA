using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class ReportsRecords
    {
        public IEnumerable<DocumentAttachmentViewModel> Records { get; set; }
        public int? SubCategory { get; set; }
        public Prioritization? Prioritization { get; set; }
        public Status? Status { get; set; }
    }
}
