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
    public class TeamController(ITeamService teamService) : ControllerBase
    {
        private readonly ITeamService _teamService = teamService;

        [Authorize]
        [HttpPost("assign-developer/{DeveloperId}/{TeamId}")]
        public async Task<IActionResult> AssignDeveloperToTeam(int DeveloperId, int TeamId)
        {
            var response = await _teamService.AssignDeveloperToTeam(DeveloperId, TeamId);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return Ok( new {response.Success , response.Message});
        }

        [Authorize]
        [HttpPost("assign-project/{ProjectId}/{TeamId}")]
        public async Task<IActionResult> AssignProjectToTeam(int ProjectId, int TeamId)
        {
            var response = await _teamService.AssignProjectToTeam(ProjectId, TeamId);

            if (!response.Success) 
            {
                return BadRequest(new { response.Success, response.Message });        
            }

            return Ok(new { response.Success, response.Message });

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromForm] TeamDTO teamDTO)
        {
            var response = await _teamService.CreateTeam(teamDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return CreatedAtAction(nameof(GetTeamById), new { id = response.Data.Id }, response.Data);
        }

        [Authorize]
        [HttpDelete("delete-developer/{TeamId}/{DeveloperId}")]
        public async Task<IActionResult> DeleteDeveloper(int TeamId, int DeveloperId)
        {
            var response = await _teamService.DeleteDeveloper(TeamId, DeveloperId);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NotFound();
        }

        [Authorize]
        [HttpDelete("delete-project/{TeamId}/{ProjectId}")]
        public async Task<IActionResult> DeleteProject(int TeamId, int ProjectId)
        {
            var response = await _teamService.DeleteProject(TeamId, ProjectId);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamById(int id)
        {
            var response = await _teamService.DeleteTeamById(id);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTeams(int Page = 1, int PageSize = 10)
        {
            var response = await _teamService.GetAllTeams(Page, PageSize);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return Ok(response.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeamById(int id)
        {
            var response = await _teamService.DeleteTeamById(id);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return Ok(response.Data);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchTeamById(int id, JsonPatchDocument<TeamDTO> patchData)
        {
            var response = await _teamService.PatchTeamById(id, patchData);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

        [Authorize]
        [HttpPut("{id}")] 
        public async Task<IActionResult> UpdateTeamById(int id, TeamDTO teamDTO)    
        {
            var response = await _teamService.UpdateTeamById(id, teamDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

    }
}
