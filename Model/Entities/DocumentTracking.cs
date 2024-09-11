using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class DocumentTracking:BaseEntity
    {
        [ForeignKey("DocsAttachment")]
        public int DocumentAttachmentId { get; set; }
        public ReviewerStatus ReviewerStatus { get; set; }
        [ForeignKey("Reviewer")]
        public string ReviewerId { get; set; }
        public  virtual DocumentAttachment DocsAttachment { get; set; }
        public virtual AppIdentityUser Reviewer { get; set; }
    }
}
