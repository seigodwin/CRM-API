using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<bool>> Register(UserDTO userDTO);
        Task<ServiceResponse<string>> Login(LoginDTO loginDTO);
        Task<ServiceResponse<bool>> ResetPassword(string toEmail, string? otp = null, string? newPassword = null, string? confirmPassword = null);     

    }
}
