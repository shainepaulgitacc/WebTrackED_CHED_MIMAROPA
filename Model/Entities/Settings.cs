namespace WebTrackED_CHED_MIMAROPA.Model.Entities
{
    public class Settings:BaseEntity
    {
       
        public string LogoFileName { get; set; }
        public bool DocumentNotif { get; set; }
        public bool RegisteredUserNotif { get; set; }
        public int PasswordRequiredLength { get; set; }
        public string EmailDomain { get; set; }
        public bool EnableRegistration { get; set; }
    }
}
