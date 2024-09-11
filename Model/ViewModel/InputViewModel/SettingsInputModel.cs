namespace WebTrackED_CHED_MIMAROPA.Model.ViewModel.InputViewModel
{
    public class SettingsInputModel:BaseInputModel
    {
        public string LogoFileName { get; set; }
        public bool DocumentNotif { get; set; }
        public bool RegisteredUserNotif { get; set; }
        public int PasswordRequiredLength { get; set; }
        public string EmailDomain { get; set; }
        public bool EnableRegistration { get; set; }
    }
}
