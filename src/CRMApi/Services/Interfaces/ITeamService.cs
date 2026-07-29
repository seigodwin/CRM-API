using CRMApi.Domain.DTOs;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;

namespace CRMApi.Services.Interfaces
{
    public interface ITeamService
    {
        Task<ServiceResponse<GetTeamDto>> CreateTeam(CreateTeamDTO teamDTO);
        Task<ServiceResponse<List<GetTeamDto>>> GetAllTeams(int Page, int PageSize);
        Task<ServiceResponse<object>> UpdateTeamById(int id , CreateTeamDTO teamDTO);
        Task<ServiceResponse<object>> DeleteTeamById(int id);
        Task<ServiceResponse<object>> DeleteDeveloper(int TeamId, string DeveloperId);
        Task<ServiceResponse<object>> DeleteProject(int TeamId, int ProjectId);
        Task<ServiceResponse<GetTeamDto>> GetTeamById(int id);
        Task<ServiceResponse<object>> PatchTeamById(int id, JsonPatchDocument<CreateTeamDTO> teamDTO);
        Task<ServiceResponse<object>> AssignDeveloperToTeam(string DeveloperId, int TeamId);
        Task<ServiceResponse<object>> AssignProjectToTeam(int ProjectId, int TeamId);  

    }
}
