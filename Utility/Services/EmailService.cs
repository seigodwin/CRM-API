using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CRMApi.Utility.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CRMApi.Utility.Services
{
    public class EmailService(IConfiguration config) : IEmailService
    {
        var KeyVaultUrl = builder.Configuration["KeyVault:KeyVaultUrl"];
        var KeyVaultClient = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCridential());

        var sengridApiKeySecret = await KeyVaultClient.GetSecretAsync("SengridApiKey");
        string sengridApiKeyValue = sengridSecret.Value.Value;
        public async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            var client = new SendGridClient(sengridApiKeyValue);
            var from = new EmailAddress("crmapi135@gmail.com", "CRM Api");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
