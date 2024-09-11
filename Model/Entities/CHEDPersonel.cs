using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class CHEDPersonel:BaseEntity
    {
        [ForeignKey("Office")]
        public int? OfficeId { get; set; }

        [ForeignKey("Designation")]
        public int? DesignationId { get; set; }
        [ForeignKey("User")]
        public string IdentityUserId { get; set; }

       //Table Mapping
        public virtual Office Office { get; set; }
        public virtual Designation Designation { get; set; }

        public virtual AppIdentityUser User { get; set; }
        public virtual ICollection<DocumentTracking> DocumentTrackings { get; set; }
    }
}
