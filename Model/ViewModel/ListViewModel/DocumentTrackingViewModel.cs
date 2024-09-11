using WebTrackED_CHED_MIMAROPA.Model.Entities;
using WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class DocumentTrackingViewModel
    {
        public DocumentAttachment DocumentAttachment { get; set; }
        public DocumentTracking DocumentTracking { get;  set; }
        public CHEDPersonel CHEDPersonel { get; set; }
        public AppIdentityUser Account { get; set; }
        public Designation Designation { get; set; }
        public Office Office { get; set; }
    }
}
