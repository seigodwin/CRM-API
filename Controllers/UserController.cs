using CRMApi.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using CRMApi.Domain.Models;
using System;
using CRMApi.Domain.DTOs;
using CRMApi.Services.Interfaces;
using CRMApi.Services.Services;

namespace CRMApi.Controllers
{
    [Route("api/v1/user")]
    [ApiController]
    public class  UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegistrationRequestDTO userDTO) 
        {
            if (userDTO is not null && ModelState.IsValid)
            {
                var response = await _userService.Register(userDTO);

                return response.Success ? Ok(response) : BadRequest(response);
            }

            return BadRequest(ModelState);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDTO)
        {
            if (loginDTO is not null && ModelState.IsValid)
            {
                var response = await _userService.Login(loginDTO);

                return response.Success ? Ok(response) : BadRequest(response);
            }
            
            return BadRequest();
        }


    
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (model is not null && ModelState.IsValid)
            {
                var response = await _userService.ForgotPassword(model);

                return response.Success ? Ok(response) : BadRequest(response);
            }

            return BadRequest(ModelState);
        }

       
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (model is not null && ModelState.IsValid)
            {
                var response = await _userService.ResetPassword(model);

                return response.Success ? Ok(response) : BadRequest(response); 
            }

            return BadRequest(ModelState);  
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO roleDTO) 
        {
            if (roleDTO is not null && ModelState.IsValid) 
            {
                var response = await _userService.AssignRoleAsync(roleDTO);

                return response.Success ? NoContent() : BadRequest(response);
            }

            return BadRequest(ModelState);

        }

    }

   
}