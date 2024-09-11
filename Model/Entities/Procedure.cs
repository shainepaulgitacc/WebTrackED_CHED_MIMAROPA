using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Procedure:BaseEntity
    {
        [ForeignKey("SubCategory")]
        public int SubCategoryId { get; set; }
        public string ProcedureTitle { get; set; }
        public string Description { get; set; }
        public virtual SubCategory SubCategory { get; set; }    
    }
}
