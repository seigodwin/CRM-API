using CRMApi.Domain.DTOs;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;

namespace CRMApi.Services.Interfaces
{
    public interface ITeamService
    {
        Task<ServiceResponse<FullTeamDTO>> CreateTeam(TeamDTO teamDTO);
        Task<ServiceResponse<List<FullTeamDTO>>> GetAllTeams(int Page, int PageSize);
        Task<ServiceResponse<object>> UpdateTeamById(int id , TeamDTO teamDTO);
        Task<ServiceResponse<object>> DeleteTeamById(int id);
        Task<ServiceResponse<object>> DeleteDeveloper(int TeamId, string DeveloperId);
        Task<ServiceResponse<object>> DeleteProject(int TeamId, int ProjectId);
        Task<ServiceResponse<FullTeamDTO>> GetTeamById(int id);
        Task<ServiceResponse<object>> PatchTeamById(int id, JsonPatchDocument<TeamDTO> teamDTO);
        Task<ServiceResponse<object>> AssignDeveloperToTeam(string DeveloperId, int TeamId);
        Task<ServiceResponse<object>> AssignProjectToTeam(int ProjectId, int TeamId);  

    }
}
