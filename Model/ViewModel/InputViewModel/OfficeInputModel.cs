

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class OfficeInputModel:BaseInputModel
    {
        [Required, DisplayName("Office Name")]
        public string OfficeName { get; set; }
    }
}
