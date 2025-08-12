using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CRMApi.Utility.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CRMApi.Utility.Services
{
    public class EmailService : IEmailService
    {
        private readonly string keyVaultUrl;

        private readonly SecretClient keyVaultClient;

       // private readonly string _sendGridApiKey;

        public EmailService(IConfiguration config)
        {
            //_sendGridApiKey = config["SendGrid:ApiKey"];

            keyVaultUrl = config["KeyVault:KeyVaultUrl"];

            if (keyVaultUrl is not null)
            {
                keyVaultClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            }
            
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextBody, string htmlBody)
        {
            var SendGridApiKey = await keyVaultClient.GetSecretAsync("SendgridApiKey");
            var SendGridApiKeyValue = SendGridApiKey.Value.Value;

            var client = new SendGridClient(SendGridApiKeyValue);
            var from = new EmailAddress("crmapi135@gmail.com", "CRM Api");
            var to = new EmailAddress(toEmail);


            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextBody, htmlBody);

            var response = await client.SendEmailAsync(msg);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
    }
}
