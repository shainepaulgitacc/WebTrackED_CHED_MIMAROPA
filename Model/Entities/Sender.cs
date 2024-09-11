using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Sender:BaseEntity
    {

        [ForeignKey("User")]
        public string IdentityUserId { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public DateTime? DateOfHire { get; set; }
        public EmploymentStatus? EmploymentStatus { get; set; }
        public string? WorkLocationOffice { get; set; }
        public string? ProjectAssignment { get; set; }
        public string? SkillCompetencies { get; set; }
        public string? AchievementsAwards { get; set; }

        public virtual AppIdentityUser User { get; set; }
        public virtual ICollection<DocumentAttachment> Documents { get; set; }
    }
}
