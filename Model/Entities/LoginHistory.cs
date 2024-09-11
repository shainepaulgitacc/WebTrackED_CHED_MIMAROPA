using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class LoginHistory:BaseEntity
    {
        [ForeignKey("User")]
        public virtual string UserId { get; set; }
        public virtual bool IsActive { get; set; }

        public virtual AppIdentityUser User { get; set; } 
    }
}
