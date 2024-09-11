using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class DocumentTrackingInputModel:BaseInputModel
    {
      
        public ReviewerStatus ReviewerStatus { get; set; }
        [DisplayName("Reviewer")]
        public int ReviewerId { get; set; }
    }
}
