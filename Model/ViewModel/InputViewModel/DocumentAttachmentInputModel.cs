using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class DocumentAttachmentInputModel:BaseInputModel
    {
        [Required,DisplayName("Category")]
        public int CategoryId { get; set; }
        [Required, DisplayName("Sub Category")]
        public int SubCategoryId { get; set; }
        [Required, DisplayName("Sender")]
		public string SenderId { get; set; }
		[DisplayName("Document")]
       
        public IFormFile? File { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }

		public DocumentType DocumentType { get; set; }
		public string? Comment { get; set; }
	
        public Prioritization? Prioritization { get; set; }
    }
   
}
