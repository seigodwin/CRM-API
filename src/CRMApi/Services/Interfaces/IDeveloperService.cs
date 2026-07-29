using CRMApi.Domain.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using CRMApi.Utility;
using CRMApi.Domain.DTOs.DeveloperDTOs;

namespace CRMApi.Services.Interfaces
{
    public interface IDeveloperService
    {   
        Task<ServiceResponse<List<GetDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10);
        Task<ServiceResponse<GetDeveloperDTO>> GetDeveloperById(string id);
        Task<ServiceResponse<string>> DeleteDeveloperById(string id);
        Task<ServiceResponse<string>> UpdateDeveloperById(string id, CreateDeveloperDto developerDTO);
        Task<ServiceResponse<string>> PatchDeveloperById(string id, JsonPatchDocument<CreateDeveloperDto> developerDTO);
        
    }   
}
