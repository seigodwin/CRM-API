using CRMApi.Domain.Models;

namespace CRMApi.Utility.Interfaces
{
    public interface IJwtTokenGenerator             
    {   
        Task<string> GenerateTokenAsync(ApplicationUser user);          
    }
}
