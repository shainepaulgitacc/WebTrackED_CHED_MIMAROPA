using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class ComposeInputModel:BaseInputModel
    {
        [Required, DisplayName("Category")]
        public int CategoryId { get; set; }
        [Required, DisplayName("Sub Category")]
        public int SubCategoryId { get; set; }
        [Required, DisplayName("Sender")]
        public string SenderId { get; set; }

        public List<string> ReviewersId { get; set; }
        [DisplayName("Document")]


        public IFormFile? File { get; set; }
        public Prioritization? Prioritization { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
		public DocumentType DocumentType { get; set; }
		public Status Status { get; set; } = Status.Pending;
        public bool IsApproved { get; set; }
        public ReviewerStatus ReviewerStatus { get; set; }
    }
}
