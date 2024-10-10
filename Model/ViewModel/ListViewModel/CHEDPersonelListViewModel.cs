using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class CHEDPersonelListViewModel
    { 
        public CHEDPersonel CHEDPersonel { get; set; }
        public Designation Designation { get; set; }
      
        public AppIdentityUser Account { get; set; }
    }
}
