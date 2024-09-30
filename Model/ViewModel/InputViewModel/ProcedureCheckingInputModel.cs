using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class ProcedureCheckingInputModel
    {
        public string? SelectedIds { get; set; }
        public string? OldReviewer { get; set; }
        public string? Note { get; set; }
        public ReviewerStatus? TrackingStatus { get; set; }
        [Required,DisplayName("New Reviewer")]
        public string NewReviewers { get; set; }
        public int DocumentId { get; set; }
    }
}
