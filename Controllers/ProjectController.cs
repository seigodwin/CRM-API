using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.ProjectDTOs;
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
    [Authorize(Roles = "Admin,Employee")]
    public class ProjectController(IProjectService projectService) : ControllerBase
    {
        private readonly IProjectService _projectService = projectService;

   
        [HttpGet]
        public async Task<IActionResult> GetAllProjects(int page = 1, int pageSize = 10)
        {
            var response = await _projectService.GetAllProjects(page , pageSize);

            return response.Success ? Ok(response) : NotFound(response);

        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var response = await _projectService.GetProjectById(id);

            return response.Success ? Ok(response) : NotFound(response); 

        }

  
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectById(int id)
        {
            var response = await _projectService.DeleteProjectById(id);

            return response.Success ? NoContent() : BadRequest(response);
        }


    
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectRequestDTO NewProjectDTO)
        {
           if (NewProjectDTO is not null && ModelState.IsValid)
           {
                var response = await _projectService.UpdateProjectById(id, NewProjectDTO);

                return response.Success ? NoContent() : BadRequest(response);
           }

            return BadRequest(ModelState);
        }

       
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProjectById(int id, JsonPatchDocument<UpdateProjectRequestDTO> patchData)
        {
          if (patchData is not null && ModelState.IsValid)
            {
                var response = await _projectService.PatchProjectById(id, patchData);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromForm] ProjectDTO NewProjectDTO)
        {
           if (NewProjectDTO is not null && ModelState.IsValid)
            {
                var response = await _projectService.CreateProject(NewProjectDTO);

                return response.Success ? CreatedAtAction(nameof(GetProjectById), new { id = response.Data?.Id }, response.Data) : BadRequest(response);
            }

            return BadRequest(ModelState);

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteTeam/{ProjectId}/{TeamId}")]
        public async Task<IActionResult> DeleteTeam(int ProjectId, int TeamId) 
        {
            var response = await _projectService.DeleteTeam(ProjectId, TeamId);

            return response.Success ? NoContent() : BadRequest(response);

        }
    }
}
