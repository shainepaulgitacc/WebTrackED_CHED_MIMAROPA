using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace WebTrackED_CHED_MIMAROPA.Model.Service
{
    public class EmailSender
    {
        private readonly EmailSettings _emailSettings;
        public EmailSender(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }
        public async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();
            message.From = new MailAddress(_emailSettings.Username);
            message.To.Add(email);
            message.Subject = subject;
            message.IsBodyHtml = _emailSettings.IsBodyHtml;
            message.Body = confirmLink;

            smtpClient.Port = _emailSettings.Port;
            smtpClient.Host = _emailSettings.Server;

            smtpClient.EnableSsl = _emailSettings.UseSSL;
            smtpClient.UseDefaultCredentials = _emailSettings.UseDefaultCredentials;
            smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Send(message);
            return true;
        }
    }
}
