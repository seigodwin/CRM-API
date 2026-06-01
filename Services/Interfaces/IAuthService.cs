using CRM_API.Domain.DTos.AuthDtos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Domain.DTOs;
using CRMApi.Utility;


namespace CRMApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> RegisterAdmin(RegisterAdminRequestDto adminDTO);
         Task<ServiceResponse<string>> RegisterDeveloper(RegisterDeveloperRequestDto userDTO);
        Task<ServiceResponse<AuthenticatedUsertDto>> Login(LoginRequestDto loginDto);
        Task<ServiceResponse<string>> ForgotPassword(ForgotPasswordRequestDto model); 
        Task<ServiceResponse<object>> ResetPassword(ResetPasswordRequestDto model);
        Task<ServiceResponse<object>> AssignRoleAsync(AssignRoleDTO model);

    }
}
