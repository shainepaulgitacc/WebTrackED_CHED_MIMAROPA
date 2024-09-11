using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class CategoryInputModel : BaseInputModel
    {
        [Required, DisplayName("Category Name")]
        public string CategoryName { get; set; }
    }
}
