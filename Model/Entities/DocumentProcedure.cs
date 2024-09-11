using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class DocumentProcedure:BaseEntity
    {
        [ForeignKey("DocumentAttachment")]
        public int DocumentAttachmentId { get; set; }
        public bool IsDone { get; set; }

        public string ProcedureTitle { get; set; }
        public string ProcedureDescription { get; set; }
        public virtual DocumentAttachment DocumentAttachment { get; set; }
    }
}
