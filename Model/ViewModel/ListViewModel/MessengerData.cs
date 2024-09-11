using WebTrackED_CHED_MIMAROPA.Model.Entities;

namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel
{
    public class MessengerData
    {
        public AppIdentityUser Recipient { get; set; }
        public AppIdentityUser Sender { get; set; }
        public Message Message { get; set; }
    }
}
