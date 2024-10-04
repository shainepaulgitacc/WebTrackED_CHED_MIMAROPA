using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class DocumentAttachmentViewModel
    {
        public DocumentTracking DocumentTracking { get; set; }
        public DocumentAttachment DocumentAttachment { get; set; }
        public CHEDPersonel Reviewer { get; set; }
        public Category Category { get; set; }
        public SubCategory SubCategory { get; set; }    
        public AppIdentityUser ReviewerAccount { get; set; }
        public AppIdentityUser SenderAccount { get; set; }
        public List<DocumentTracking> DocumentTrackings{ get; set; }
        public Office Office { get; set; }
        public Designation Designation { get; set; }
    }
}
