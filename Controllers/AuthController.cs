using CRMApi.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using CRMApi.Domain.Models;
using System;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Services.Interfaces;
using CRMApi.Services.Services;
using CRM_API.Domain.DTos.AuthDtos;
using CRMApi.Domain.DTOs.AuthDtos;


namespace CRMApi.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class  AuthController : ControllerBase
    {
        private readonly IAuthService _userService;

        public AuthController(IAuthService userService)
        {
            _userService = userService;
        }

    
        [HttpPost("register-developer")]
        public async Task<IActionResult> RegisterDeveloper([FromBody] RegisterDeveloperRequestDto dto) 
        {
            if (dto is not null && ModelState.IsValid)
            {
                var response = await _userService.RegisterDeveloperAsync(dto);

                return response.Success ? CreatedAtRoute("GetDeveloperById", new{id = response.Data!.Id},response) 
                : BadRequest(response); 
            }

            return BadRequest(ModelState); 
        } 

        
    
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequestDto dto) 
        {
            if (dto is not null && ModelState.IsValid)
            {
                var response = await _userService.RegisterAdminAsync(dto);

                return response.Success ? Ok(response) : BadRequest(response);
            }

            return BadRequest(ModelState);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDTO)
        {
            if (loginDTO is not null && ModelState.IsValid)
            {
                var response = await _userService.LoginAsync(loginDTO);

                return response.Success ? Ok(response) : BadRequest(response);
            }
            
            return BadRequest();
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (model is not null && ModelState.IsValid)
            {
                var response = await _userService.ForgotPasswordAsync(model);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

       
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (model is not null && ModelState.IsValid)
            {
                var response = await _userService.ResetPasswordAsync(model);

                return response.Success ? NoContent() : BadRequest(response); 
            }

            return BadRequest(ModelState);  
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto model)
        {
            if (model is not null && ModelState.IsValid)
            {
                var response = await _userService.ChangePasswordAsync(model);

                return response.Success ? NoContent() : BadRequest(response); 
            }

            return BadRequest(ModelState);  
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto roleDTO) 
        {
            if (roleDTO is not null && ModelState.IsValid) 
            {
                var response = await _userService.AssignRolesAsync(roleDTO);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfrimEmail([FromBody] ConfirmEmailRequestDto roleDTO) 
        {
            if (roleDTO is not null && ModelState.IsValid) 
            {
                var response = await _userService.ConfirmEmailAsync(roleDTO);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] RolesRequestDto roleDTO) 
        {
            if (roleDTO is not null && ModelState.IsValid) 
            {
                var response = await _userService.CreateRoleAsync(roleDTO);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("remove-role/{Id}")]
        public async Task<IActionResult> RemoveRole(string Id) 
        {
            if (!string.IsNullOrEmpty(Id)) 
            {
                var response = await _userService.RemoveRoleAsync(Id);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest();
        }
    }

}