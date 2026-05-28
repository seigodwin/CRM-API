using Azure.Identity;
using CRMApi.Utility.Interfaces;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CRMApi.Utility.Services
{
    public class EmailService : IEmailService
    {
       private AppSettings _appSettings;

        public EmailService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextBody, string htmlBody)
        {
            var SendGridApiKeyValue = _appSettings.SendGridApiKey;

            var client = new SendGridClient(SendGridApiKeyValue);
            var from = new EmailAddress(_appSettings.FromEmail, "CRM Api");
            var to = new EmailAddress(toEmail);


            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, htmlBody);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
