using CRMApi.Domain.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using CRMApi.Utility;
using CRMApi.Domain.DTOs.DeveloperDTOs;

namespace CRMApi.Services.Interfaces
{
    public interface IDeveloperService
    {
        Task<ServiceResponse<List<FullDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10);
        Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(string id);
        Task<ServiceResponse<string>> DeleteDeveloperById(string id);
        Task<ServiceResponse<string>> UpdateDeveloperById(string id, UpdateDevRequestDTO developerDTO);
        Task<ServiceResponse<string>> PatchDeveloperById(string id, JsonPatchDocument<PatchDevRequestDTO> developerDTO);
        
    }
}
