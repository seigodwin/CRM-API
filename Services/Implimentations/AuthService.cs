
using CRM_API.Domain.DTos.AuthDtos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.DbContexts;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static System.Net.WebRequestMethods;

namespace CRMApi.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext context,UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ServiceResponse<string>> AssignRolesAsync(AssignRoleRequestDto model)
        {
            var response = new ServiceResponse<string>();
            if(model is null)
            {
                response.Success = false;
                response.Message = "Provide valid roles to continue";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user is null)
            {
                response.Message = "User not found";
                response.Success = false;
                return response;
            }

            if (model.Roles.Any())
                {
                    var cleanRoles = model.Roles
                    .Where( r => !string.IsNullOrEmpty(r))
                    .ToList();

                    if (!cleanRoles.Any())
                    {
                        response.Message = "User created but no valid roles were created to assign";
                        return response;
                    }

                    var rolesToAdd = cleanRoles.Except(await _userManager.GetRolesAsync(user)).ToList();
                    var rolesResults = await _userManager.AddToRolesAsync(user,cleanRoles);

                    if (!rolesResults.Succeeded)
                    {
                        response.Message = string.Join(Environment.NewLine,
                        rolesResults.Errors.Select(e => e.Description));
                        return response;
                    }
                    
                    response.Message = "User created, roles assign successfully";
                }
                return response;
        }

        public async Task<ServiceResponse<string>> ChangePasswordAsync(ChangePasswordRequestDto model)
        {
            var response = new ServiceResponse<string>();
            if(model is null)
            {
                response.Success = false;
                response.Message = "Provide valid data to change password";
                return response;
            }
            if(model.NewPassword != model.ConFirmNewPassword)
            {
                response.Success = false;
                response.Message = "New password does not match";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user is null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            try
            {
                var results = await _userManager
                .ChangePasswordAsync(user,model.CurrentPassword,model.NewPassword);
                if (!results.Succeeded)
                {
                    response.Message = string.Join(Environment.NewLine,
                    results.Errors.Select(e => e.Description));
                    response.Success = false;
                    return response;
                }
                response.Message = "Password change successfully";
            }

            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to change password: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ConfirmEmailAsync(ConfirmEmailRequestDto model)
        {
            var response = new ServiceResponse<string>();
            if(model is null)
            {
                response.Success = false;
                response.Message = "Provide valid data to continue";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user is null)
            {
                response.Success = false;
                response.Message = "User not found";
                return response;
            }

            try
            {
                var results = await _userManager.ConfirmEmailAsync(user,model.Token);
                if (!results.Succeeded)
                {
                    response.Message = string.Join(Environment.NewLine,
                    results.Errors.Select( e => e.Description));
                    return response;
                }
                response.Message = "Email Confirmed successfully";
            }

            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to verify email: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto model)
        {
            var response = new ServiceResponse<string>();

            if(model is null)
            {
                response.Success = false;
                response.Message = "Provide your email to continue";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user is null)
            {
                response.Success = false;
                response.Message = "A password reset link has been sent to the provided email.";
                return response;
            }
            try
            {
                 var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if(token is not null && !string.IsNullOrWhiteSpace(token))
                {
                    response.Data = token;
                    response.Message = "Password reset token generated";
                }
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to generate password reset token: {ex.Message}";
            }
           
           return response;
        }

        public async Task<ServiceResponse<AuthenticatedUsertDto>> LoginAsync(LoginRequestDto loginDto)
        {
            var response = new ServiceResponse<AuthenticatedUsertDto>();

            if(loginDto is null)
            {
                response.Success = false;
                response.Message = "Please provide login details to continue";
                return response;
            }

            var user = await _userManager.FindByIdAsync(loginDto.Email);
            if(user is null)
            {
                response.Message = "Usernot found";
                response.Success = false;
                return response;
            }

            if(await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                try
                {
                    var token = await _jwtTokenGenerator.GenerateTokenAsync(user);
                    response.Message = "Login successful";
                    response.Data = new AuthenticatedUsertDto
                    {
                        Username = user.UserName!,
                        Token = token
                    };
                }
                catch(Exception ex)
                {
                    response.Success = false;
                    response.Message = $"Failed to generate token:{ex.Message}";
                }
            }

            else
            {
                response.Message = "Incorrect email or password";
                response.Success = false;
            }
            return response;
        }

        public async Task<ServiceResponse<string>> RegisterAdminAsync(RegisterAdminRequestDto adminDTO)
        {
            var response = new ServiceResponse<string>();
            if(adminDTO is null)
            {
                response.Success = false;
                response.Message = "Provide registeration data to continue";
                return response;
            }

            var admin = new Admin
            {
                FirstName = adminDTO.FirstName,
                LastName = adminDTO.LastName,
                Email = adminDTO.Email,
                UserName = adminDTO.Username ?? adminDTO.Email
            };

            try
            {
                await _userManager.CreateAsync(admin,adminDTO.Password);
                await _context.SaveChangesAsync();
                response.Message = "User created successfully";
                
                if (adminDTO.Roles.Any())
                {
                    var cleanRoles = adminDTO.Roles
                    .Where( r => !string.IsNullOrEmpty(r))
                    .ToList();

                    if (!cleanRoles.Any())
                    {
                        response.Message = "User created but no valid roles were created to assign";
                        return response;
                    }

                    var rolesToAdd = cleanRoles.Except(await _userManager.GetRolesAsync(admin)).ToList();
                    var rolesResults = await _userManager.AddToRolesAsync(admin,cleanRoles);

                    if (!rolesResults.Succeeded)
                    {
                        response.Message = string.Join(Environment.NewLine,
                        rolesResults.Errors.Select(e => e.Description));
                        return response;
                    }

                    response.Message = "User created, roles assign successfully";
                }
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to register new user: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<RegisterDeveloperResponseDto>> RegisterDeveloperAsync(RegisterDeveloperRequestDto userDTO)
        {
            var response = new ServiceResponse<RegisterDeveloperResponseDto>();
            if(userDTO is null)
            {
                response.Success = false;
                response.Message = "Provide registeration data to continue";
                return response;
            }

            var developer = new Developer
            {
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Email = userDTO.Email,
                UserName = userDTO.UserName ?? userDTO.Email
            };

            try
            {
                await _userManager.CreateAsync(developer,userDTO.Password);
                await _context.SaveChangesAsync();
                response.Message = "User created successfully";

                if (userDTO.Roles.Any())
                {
                    var cleanRoles = userDTO.Roles
                    .Where( r => !string.IsNullOrEmpty(r))
                    .ToList();

                    if (!cleanRoles.Any())
                    {
                        response.Message = "User created but no valid roles were created to assign";
                        return response;
                    }

                    var rolesToAdd = cleanRoles.Except(await _userManager.GetRolesAsync(developer)).ToList();
                    var rolesResults = await _userManager.AddToRolesAsync(developer,cleanRoles);

                    if (!rolesResults.Succeeded)
                    {
                        response.Message =
                            "User created but roles assignment failed: " +
                            string.Join(", ", rolesResults.Errors.Select(e => e.Description));
                            return response;
                    }
                    response.Message = "User created, roles assign successfully";
                }

                response.Data = new RegisterDeveloperResponseDto
                {
                    Id = developer.Id,
                    FirstName = developer.FirstName,
                    LastName = developer.LastName,
                    Email = developer.Email,
                    Roles = (await _userManager.GetRolesAsync(developer)).ToList(),
                    Stack = developer.Stack
                };
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to register new user: {ex.Message}";
            }
            
            return response;
        }

        public async Task<ServiceResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto model)
        {
            var response = new ServiceResponse<string>();
            if(model is null)
            {
                response.Success = false;
                response.Message = "Provide password request data to continue";
                return response;
            }

            if(model.NewPassword != model.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "Passwords do not match";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user is null)
            {
                response.Success = false;
                response.Message = "The provided email does not have an account";
                return response;
            }

            try
            {
                var changedPassword = await _userManager.ResetPasswordAsync(user,model.Token,model.NewPassword);
                if (!changedPassword.Succeeded)
                {
                    response.Success = false;
                    response.Message = $"Failed to reset password: {changedPassword.Errors.FirstOrDefault()}";
                }
                response.Message = "Password reset success";
            }

            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to reset password: {ex.Message}";
            }
            return response;
        }
    }
}

        