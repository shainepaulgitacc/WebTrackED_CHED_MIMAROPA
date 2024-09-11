namespace WebTrackED_CHED_MIMAROPA.Model.Service
{
    public class EmailSettings
    {
        public string Server { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public bool UseSSL { get; set; }
        public bool IsBodyHtml { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
