
using CRMApi.Domain.DTOs;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;

namespace CRMApi.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<GetProjectDto>>> GetAllProjects(int page = 1, int pageSize = 10);   
        Task<ServiceResponse<GetProjectDto>> GetProjectById(int id);
        Task<ServiceResponse<object>> DeleteProjectById(int id);
        Task<ServiceResponse<object>> DeleteTeam(int projectId, int teamId);
        Task<ServiceResponse<object>> UpdateProjectById(int id, ProjectDTO projectDTO);
        Task<ServiceResponse<object>> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData);
        Task<ServiceResponse<GetProjectDto>> CreateProject(ProjectDTO projectDTO);

    }
}
