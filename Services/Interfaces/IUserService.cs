using CRMApi.Domain.DTOs;
using CRMApi.Utility;


namespace CRMApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<object>> Register(RegistrationRequestDTO userDTO);
        Task<ServiceResponse<LoginResponseDTO>> Login(LoginRequestDTO loginDTO);
        Task<ServiceResponse<ForgotPasswordResponseDTO>> ForgotPassword(ForgotPasswordRequestDTO model); 
        Task<ServiceResponse<object>> ResetPassword(ResetPasswordDTO model);
        Task<ServiceResponse<object>> AssignRoleAsync(AssignRoleDTO model);

    }
}
