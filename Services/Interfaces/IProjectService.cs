using Azure;
using CRMApi.Domain.DTOs;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ServiceResponse<List<FullProjectDTO>>> GetAllProjects(int page = 1, int pageSize = 10);   
        Task<ServiceResponse<FullProjectDTO>> GetProjectById(int id);
        Task<ServiceResponse<bool>> DeleteProjectById(int id);
        Task<ServiceResponse<bool>> DeleteTeam(int projectId, int teamId);
        Task<ServiceResponse<bool>> UpdateProjectById(int id, ProjectDTO projectDTO);
        Task<ServiceResponse<bool>> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData);
        Task<ServiceResponse<FullProjectDTO>> CreateProject(ProjectDTO projectDTO);

    }
}
