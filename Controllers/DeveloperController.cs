
using CRMApi.Domain.DTOs;
using CRMApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace CRMApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DeveloperController(IDeveloperService developerService) : ControllerBase
    {
        private readonly IDeveloperService _developerService = developerService;
        

        [HttpGet]
        public async Task<IActionResult> GetAllDevelopers(int page = 1,  int pageSize = 10)
        {
            var serviceResponse = await _developerService.GetAllDevelopers(page , pageSize);

            if (!serviceResponse.Success)
            {
                return NotFound(new { serviceResponse.Success, serviceResponse.Message });
            }

            return Ok(serviceResponse.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeveloperById(int id)
        {
            var serviceResponse = await _developerService.GetDeveloperById(id);

            if (!serviceResponse.Success)
            {
                return NotFound(new { serviceResponse.Success, serviceResponse.Message });
            }

            return Ok(serviceResponse.Data);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeveloperById(int id)
        {
            var serviceResponse = await _developerService.DeleteDeveloperById(id);

            if (!serviceResponse.Success)
            {
                return NotFound(new { serviceResponse.Success, serviceResponse.Message });
            }

            return NoContent();
        }


        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateDeveloper([FromForm] DeveloperDTO developerDTO)
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

            return CreatedAtAction(nameof(GetDeveloperById), new { id = serviceResponse.Data.Id }, serviceResponse.Data);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeveloper(int id, DeveloperDTO UpdatedDeveloperDTO)
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

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchDeveloperById (int id, JsonPatchDocument<DeveloperDTO> patchData)
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
