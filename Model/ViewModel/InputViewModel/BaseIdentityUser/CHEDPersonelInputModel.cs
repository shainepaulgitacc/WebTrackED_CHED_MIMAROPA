using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel.BaseIdentityUser
{
    public class CHEDPersonelInputModel : BaseIdentityUserInputModel
    {
        public int? ReviewerId { get; set; }
        [DisplayName("Designation")]
        public int? DesignationId { get; set; }


    }
}
