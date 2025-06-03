using CRMApi.Domain.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using CRMApi.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using CRMApi.Utility.Services;

namespace CRMApi.Services.Interfaces
{
    public interface IDeveloperService
    {
        Task<ServiceResponse<List<FullDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10);
        Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(int id);
        Task<ServiceResponse<bool>> DeleteDeveloperById(int id);
        Task<ServiceResponse<bool>> UpdateDeveloperById(int id, DeveloperDTO developerDTO);
        Task<ServiceResponse<bool>> PatchDeveloperById(int id, JsonPatchDocument<DeveloperDTO> developerDTO);
        Task<ServiceResponse<FullDeveloperDTO>> CreateDeveloper(DeveloperDTO developerDTO);

    }
}
