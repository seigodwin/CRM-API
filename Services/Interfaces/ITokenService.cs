using CRM_API.Domain.DTos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Domain.Models;

namespace CRMApi.Utility.Interfaces
{
    public interface ITokenService           
    {   
        Task<AuthenticatedUsertDto> GenerateTokenPairAsync(ApplicationUser user);
        Task<AuthenticatedUsertDto> RefreshTokenAsync(RefreshRequestDto request);  
    }
}
