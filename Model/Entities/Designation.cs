using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Designation:BaseEntity
    {
        public string DesignationName { get; set; }
        public virtual ICollection<CHEDPersonel> Reviewers { get; set; }
    }
}
