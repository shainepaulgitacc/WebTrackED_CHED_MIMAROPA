using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class ProcedureCheckingInputModel
    {
        public string? SelectedIds { get; set; }
        public string? OldReviewer { get; set; }
        [Required,DisplayName("New Reviewer")]
        public string NewReviewers { get; set; }
        public int DocumentId { get; set; }
    }
}
