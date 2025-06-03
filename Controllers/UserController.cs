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
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        
        [HttpPost("sign-up")]
        public async Task<IActionResult> Register([FromForm] UserDTO userDTO) 
        {
            if (userDTO is null)
            {
                return BadRequest("User model is null");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.Register(userDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            
            return Ok("User created successfully");
        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (loginDTO is null)
            {
                return BadRequest("User model is null");        
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _userService.Login(loginDTO);

            if (!response.Success)
            {
                return BadRequest(new { response.Success, response.Message });
            }

            return Ok(new {response.Data});
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            var response = await _userService.ResetPassword(model.Email, model.OTP, model.NewPassword, model.ConfirmPassword);
            return response.Success ? Ok(response) : BadRequest(response.Message);
        }


    }

   
}