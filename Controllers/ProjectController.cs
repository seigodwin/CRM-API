using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        private readonly IProjectService _projectService = projectService;


        [HttpGet]
        public async Task<IActionResult> GetAllProjects(int page = 1, int pageSize = 10)
        {
            var response = await _projectService.GetAllProjects(page, pageSize);

            if (!response.Success)
            {
                return NotFound(new { response.Success, response.Message });
            }

            return Ok(response.Data);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var response = await _projectService.GetProjectById(id);

            if (!response.Success)
            {
                return NotFound(new { response.Success, response.Message });
            }

            return Ok(response.Data);

        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectById(int id)
        {
            var response = await _projectService.DeleteProjectById(id);

            if (!response.Success)
            {
                return NotFound(new { response.Success, response.Message });
            }

            return NoContent();
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectDTO NewProjectDTO)
        {
            if (NewProjectDTO is null)
            {
                return BadRequest("Project data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _projectService.UpdateProjectById(id, NewProjectDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData)
        {
            if (patchData is null)
            {
                return BadRequest("Patch data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _projectService.PatchProjectById(id, patchData);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] ProjectDTO NewProjectDTO)
        {
            if (NewProjectDTO is null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _projectService.CreateProject(NewProjectDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return CreatedAtAction(nameof(GetProjectById), new { id = response.Data.Id }, response.Data);

        }

        [HttpDelete("delete-team/{ProjectId}/{TeamId}")]
        public async Task<IActionResult> DeleteTeam(int ProjectId, int TeamId)
        {
            var response = await _projectService.DeleteTeam(ProjectId, TeamId);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();
        }
    }
}
