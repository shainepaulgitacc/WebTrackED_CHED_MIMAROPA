using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class SubCategoryInputModel:BaseInputModel
    {
       
        [Required, DisplayName("Subcategory Name")]
        public string SubCategoryName { get; set; }
    }
}
