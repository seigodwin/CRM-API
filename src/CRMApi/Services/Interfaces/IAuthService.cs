using CRM_API.Domain.DTos;
using CRM_API.Domain.DTos.AuthDtos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.AuthDtos;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Utility;


namespace CRMApi.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<string>> RegisterAdminAsync(RegisterAdminRequestDto adminDTO);
        Task<ServiceResponse<CreateDeveloperDto>> RegisterDeveloperAsync(CreateDeveloperDto userDTO);
        Task<ServiceResponse<AuthenticatedUsertDto>> LoginAsync(LoginRequestDto loginDto);
        Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto model); 
        Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto model);
        Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordRequestDto model);
        Task<ServiceResponse<string>> ConfirmEmailAsync(ConfirmEmailRequestDto model);
        Task<ServiceResponse<string>> AssignRolesAsync(AssignRoleRequestDto model);
        Task<ServiceResponse<string>> CreateRoleAsync(RolesRequestDto model);
        Task<ServiceResponse<string>> RemoveRoleAsync(string Id);
        Task<ServiceResponse<AuthenticatedUsertDto>> RefreshTokenAsync(RefreshRequestDto request);
    }
}
