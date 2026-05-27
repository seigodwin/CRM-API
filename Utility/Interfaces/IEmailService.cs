namespace CRMApi.Utility.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string plainTextBody, string htmlBody); 
    }
}
