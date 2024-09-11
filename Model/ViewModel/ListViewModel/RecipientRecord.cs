using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class RecipientRecord
    {
        public int Count { get; set; }
        public AppIdentityUser Recipient { get; set; }
        public Message Message { get; set; }
    }
}
