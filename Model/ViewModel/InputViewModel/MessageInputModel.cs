namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class MessageInputModel:BaseInputModel
    {
        public string Sender { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public string Recipient { get; set; }
    }
}
