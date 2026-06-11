using CRMApi.Domain.Models;
using CRMApi.Options;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Resend;

namespace CRMApi.Services.Implimentations
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly IResend _resend;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmailService(IOptions<EmailOptions> emailOptions, IResend resend, UserManager<ApplicationUser> userManager) 
        {
            _emailOptions = emailOptions.Value;
            _resend = resend;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<string>> ResetPasswordRequestEmailAsync(string toEmail , string userName, string token)
        {
            var response = new ServiceResponse<string>();

           if(string.IsNullOrEmpty(toEmail))
            {
                response.Success = false;
                response.Message = "Provide a valid email to continue";
                return response;
           }


            if (string.IsNullOrEmpty(token))
            {
                response.Success = false;
                response.Message = "Provide a valid token to continue";
                return response;
            }

            var message = new EmailMessage()
            {
                To = toEmail,
                From = _emailOptions.FromEmail,
                Subject = "Reset Password Request",
                HtmlBody = $"""
    <div style="font-family: Arial, sans-serif; line-height: 1.6;">
        <h1>Hello {userName ?? toEmail},</h1>

        <p>You requested to reset your password.</p>

        <p>
            Click the button below to reset your password:
        </p>

        <a 
            href="{token}" 
            style="
                display: inline-block;
                padding: 12px 20px;
                background-color: #2563eb;
                color: white;
                text-decoration: none;
                border-radius: 6px;
                font-weight: bold;
            ">
            Reset Password
        </a>

        <p style="margin-top: 20px;">
            If you did not request this, please ignore this email or contact support immediately.
        </p>

        <p>
            This link may expire after some time for security reasons.
        </p>
    </div>
    """
            };

            var emailSent = await _resend.EmailSendAsync(message);

            if (emailSent is null)
            {
                response.Success = false;
                response.Message = $"Failed to send email: {emailSent?.Exception}";
                return response;
            }

            response.Message = "Reset password request email sent";
            return response;
        }

        public async Task<ServiceResponse<string>> ResetPasswordResponseEmailAsync(string toEmail,string userName)
        {
            var response = new ServiceResponse<string>();

            if (string.IsNullOrEmpty(toEmail))
            {
                response.Success = false;
                response.Message = "If a user exists, a password reset link has been sent to their email";
                return response;
            }


            var message = new EmailMessage()
            {
                To = toEmail,
                From = _emailOptions.FromEmail,
                Subject = "Reset Password Successful",
                HtmlBody = $"""
                            <h1>Hello {userName ?? toEmail},</h1>
                            <p>Your password has been successfully reset.</p>
                            <p>If this was not you, please contact support immediately.</p>
                            """
            };

            var emailSent = await _resend.EmailSendAsync(message);

            if (emailSent is null)
            {
                response.Success = false;
                response.Message = $"Failed to send email: {emailSent?.Exception}";
                return response;
            }

            response.Message = "Reset password success email sent";
            return response;
        }

        public async Task<ServiceResponse<string>> WelcomeEmailAsync(string toEmail , string userName)
        {
            var response = new ServiceResponse<string>();

            if (string.IsNullOrEmpty(toEmail))
            {
                response.Success = false;
                response.Message = "Email is null";
                return response;
            }

            var message = new EmailMessage()
            {
                To = toEmail,
                From = _emailOptions.FromEmail,
                Subject = "Welcome",
                HtmlBody = $"""
                            <h1>Hello {userName ?? toEmail},</h1>
                            <p>Thanks for creating an account with us.</p>
                            <p>We look forward to working with you.</p>
                            """
            };

            var emailSent = await _resend.EmailSendAsync(message);

            if (emailSent is null)
            {
                response.Success = false;
                response.Message = $"Failed to send email: {emailSent?.Exception}";
                return response;
            }

            response.Message = "Reset password success email sent";
            return response;
        }
    }
    
}
