
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace CRMApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DeveloperController(IDeveloperService developerService) : ControllerBase
    {
        private readonly IDeveloperService _developerService = developerService;


        
        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetAllDevelopers(int page = 1,  int pageSize = 10)
        {
            var serviceResponse = await _developerService.GetAllDevelopers(page , pageSize);

            if (!serviceResponse.Success)
            {
                return NotFound(new { serviceResponse.Success, serviceResponse.Message });
            }

            return Ok(serviceResponse);
        }

        
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> GetDeveloperById(string id)
        {
            var serviceResponse = await _developerService.GetDeveloperById(id);

            if (!serviceResponse.Success)
            {
                return NotFound(new {serviceResponse.Message });
            }

            return Ok(serviceResponse);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeveloperById(string id)
        {
            var serviceResponse = await _developerService.DeleteDeveloperById(id);

            if (!serviceResponse.Success)
            {
                return NotFound(new {serviceResponse.Message });
            }

            return NoContent();
        }


      
        [HttpPost]
        public async Task<IActionResult> CreateDeveloper([FromForm] DevRegistrationRequestDTO developerDTO)
        {
            
           if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var serviceResponse = await _developerService.CreateDeveloper(developerDTO);

            if (!serviceResponse.Success)
            {
                return BadRequest(new { serviceResponse.Success, serviceResponse.Message });
            }

            return CreatedAtAction(nameof(GetDeveloperById), new { id = serviceResponse.Data?.Id }, serviceResponse.Data);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeveloper(string id, UpdateDevRequestDTO UpdatedDeveloperDTO)
        {
            if (UpdatedDeveloperDTO is null)
            {
                return BadRequest("Developer data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  
            }

            var serviceResponse = await _developerService.UpdateDeveloperById(id, UpdatedDeveloperDTO);

            if (!serviceResponse.Success)
            {
                return BadRequest(new { serviceResponse.Success, serviceResponse.Message });
            }

            return NoContent();
        }


        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDeveloperById (string id, JsonPatchDocument<PatchDevRequestDTO> patchData)
        {
            if (patchData is null)
            {
                return BadRequest("Patch data is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _developerService.PatchDeveloperById(id, patchData);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return NoContent();

        }

        
    }
}
