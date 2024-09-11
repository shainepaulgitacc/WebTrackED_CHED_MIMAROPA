using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class ProcedureInputModel:BaseInputModel
    {
        public int SubCategoryId { get; set; }
        [Required,DisplayName("Procedure Title")]
        public string ProcedureTitle { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
