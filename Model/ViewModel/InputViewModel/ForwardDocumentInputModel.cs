using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class ForwardDocumentInputModel : BaseInputModel
    {
        public int DocumentId { get; set; }
        public ReviewerStatus TrackingStatus { get; set; }
        public string? Note { get; set; }
        public string NewReviewers { get; set; }
    }
}
