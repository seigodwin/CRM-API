
using CRMApi.Domain.DTOs;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;

namespace CRMApi.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<FullProjectDTO>>> GetAllProjects(int page = 1, int pageSize = 10);   
        Task<ServiceResponse<FullProjectDTO>> GetProjectById(int id);
        Task<ServiceResponse<object>> DeleteProjectById(int id);
        Task<ServiceResponse<object>> DeleteTeam(int projectId, int teamId);
        Task<ServiceResponse<object>> UpdateProjectById(int id, ProjectDTO projectDTO);
        Task<ServiceResponse<object>> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData);
        Task<ServiceResponse<FullProjectDTO>> CreateProject(ProjectDTO projectDTO);

    }
}
