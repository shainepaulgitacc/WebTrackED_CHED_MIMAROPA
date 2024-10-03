using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class SubCategory:BaseEntity
    {
        public string SubCategoryName { get; set; }

        public virtual ICollection<DocumentAttachment> Documents { get; set; }
    }
}
