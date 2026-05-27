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
        Task<ServiceResponse<object>> DeleteDeveloperById(string id);
        Task<ServiceResponse<object>> UpdateDeveloperById(string id, UpdateDevRequestDTO developerDTO);
        Task<ServiceResponse<object>> PatchDeveloperById(string id, JsonPatchDocument<PatchDevRequestDTO> developerDTO);
        Task<ServiceResponse<FullDeveloperDTO>> CreateDeveloper(DevRegistrationRequestDTO developerDTO);
        Task<ServiceResponse<LoginResponseDTO>> Login(DeveloperLoginRequestDTO model);

    }
}
