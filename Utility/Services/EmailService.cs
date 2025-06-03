using SendGrid.Helpers.Mail;
using SendGrid;
using CRMApi.Utility.Interfaces;

namespace CRMApi.Utility.Services
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        private readonly string _sendGridApiKey = config["SendGrid:ApiKey"];
        public async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress("crmapi135@gmail.com", "CRM Api");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
