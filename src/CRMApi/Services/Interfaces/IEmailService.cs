using CRMApi.Utility;

namespace CRMApi.Services.Interfaces
{
    public interface IEmailService
    {
        Task<ServiceResponse<string>> WelcomeEmailAsync(string toEmail , string userName);  
        Task<ServiceResponse<string>> ResetPasswordRequestEmailAsync(string toEmail , string userName, string token);  
        Task<ServiceResponse<string>> ResetPasswordResponseEmailAsync(string toEmail , string userName);  
    }
}
