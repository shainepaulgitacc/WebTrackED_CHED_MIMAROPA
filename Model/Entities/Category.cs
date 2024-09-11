namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Category:BaseEntity
    {
        public string CategoryName { get; set; }
        public virtual ICollection<DocumentAttachment> Documents { get; set; }
    }
}
