using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services;
using CRMApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CRMApi.Controllers 
{
    [Route("api/v1/[controller]")] 
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class TeamController(ITeamService teamService) : ControllerBase 
    {
        private readonly ITeamService _teamService = teamService; 

        
        [HttpPost("assign-developer/{DeveloperId}/{TeamId}")] 
        public async Task<IActionResult> AssignDeveloperToTeam(string DeveloperId, int TeamId) 
        {
            var response = await _teamService.AssignDeveloperToTeam(DeveloperId, TeamId); 

            return response.Success ? NoContent() : BadRequest(response);  
        }


        [HttpPost("assign-project/{ProjectId}/{TeamId}")]
        public async Task<IActionResult> AssignProjectToTeam(int ProjectId, int TeamId)
        {
            var response = await _teamService.AssignProjectToTeam(ProjectId, TeamId);

            return response.Success? NoContent() : BadRequest(response);

        }


        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromForm] TeamDTO teamDTO)
        {
            var response = await _teamService.CreateTeam(teamDTO);

            return response.Success ? CreatedAtAction(nameof(GetTeamById), new { id = response.Data?.Id }, response.Data) : BadRequest(response);
        }



        [HttpDelete("delete-developer/{TeamId}/{DeveloperId}")]
        public async Task<IActionResult> DeleteDeveloper(int TeamId, string DeveloperId)
        {
            var response = await _teamService.DeleteDeveloper(TeamId, DeveloperId);

            return response.Success ? NoContent() : BadRequest(response);
        }


        [HttpDelete("delete-project/{TeamId}/{ProjectId}")]
        public async Task<IActionResult> DeleteProject(int TeamId, int ProjectId)
        {
            var response = await _teamService.DeleteProject(TeamId, ProjectId);

            return response.Success ? NoContent() : BadRequest(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamById(int id)
        {
            var response = await _teamService.DeleteTeamById(id);

            return response.Success ? NoContent() : BadRequest(response);
        }


        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> GetAllTeams(int Page = 1, int PageSize = 10)
        {
            var response = await _teamService.GetAllTeams(Page, PageSize);

            return response.Success ? Ok(response) : NotFound(response);
        }


        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeamById(int id)
        {
            var response = await _teamService.DeleteTeamById(id);

            return response.Success ? Ok(response) : NotFound(response); 
        }



        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTeamById(int id, JsonPatchDocument<TeamDTO> patchData)
        {
            var response = await _teamService.PatchTeamById(id, patchData);

            return response.Success ? NoContent() : BadRequest(response);
        }



        [HttpPut("{id}")] 
        public async Task<IActionResult> UpdateTeamById(int id, TeamDTO teamDTO)    
        {
            var response = await _teamService.UpdateTeamById(id, teamDTO);

            return response.Success ? NoContent() : BadRequest(response);
        }

    }
}
