using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class DesignationInputModel:BaseInputModel
    {
       
        [Required,DisplayName("Designation Name")]
        public string DesignationName { get; set; }
    }
}
