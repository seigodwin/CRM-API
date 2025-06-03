using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.JsonPatch;

namespace CRMApi.Services.Interfaces
{
    public interface ITeamService
    {
        Task<ServiceResponse<FullTeamDTO>> CreateTeam(TeamDTO teamDTO);
        Task<ServiceResponse<List<FullTeamDTO>>> GetAllTeams(int Page, int PageSize);
        Task<ServiceResponse<bool>> UpdateTeamById(int id , TeamDTO teamDTO);
        Task<ServiceResponse<bool>> DeleteTeamById(int id);
        Task<ServiceResponse<bool>> DeleteDeveloper(int TeamId, int DeveloperId);
        Task<ServiceResponse<bool>> DeleteProject(int TeamId, int ProjectId);
        Task<ServiceResponse<FullTeamDTO>> GetTeamById(int id);
        Task<ServiceResponse<bool>> PatchTeamById(int id, JsonPatchDocument<TeamDTO> teamDTO);
        Task<ServiceResponse<bool>> AssignDeveloperToTeam(int DeveloperId, int TeamId);
        Task<ServiceResponse<bool>> AssignProjectToTeam(int ProjectId, int TeamId);  

    }
}
