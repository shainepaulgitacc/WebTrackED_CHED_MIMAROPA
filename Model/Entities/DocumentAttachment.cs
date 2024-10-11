using Microsoft.CodeAnalysis.CSharp;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class DocumentAttachment:BaseEntity
    {

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        [ForeignKey("SubCategory")]
        public int SubCategoryId { get; set; }
        //[ForeignKey("Sender")]
        public string SenderId { get; set; }
        public string FileName { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }

        public DocumentType DocumentType { get; set; }
       // public Status Status { get; set; } = Status.Pending;
        public Prioritization? Prioritization { get; set; }
        public string? Comment { get; set; }
     
        public virtual ICollection<DocumentTracking> DocumentTrackings { get; set; }
        public virtual Category Category { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        //public virtual AppIdentityUser Sender { get; set; }
    }

}
