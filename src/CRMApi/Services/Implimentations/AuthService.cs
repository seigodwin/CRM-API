
using CRM_API.Domain.DTos;
using CRM_API.Domain.DTos.AuthDtos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs.AuthDtos;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


namespace CRMApi.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEmailService _eMailService;

        public AuthService(AppDbContext context,UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, IRateLimitService rateLimitService,
        ITokenService tokenService, IEmailService emailService)
        {
            _rateLimitService = rateLimitService;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
            _eMailService = emailService;
        }

        public async Task<ServiceResponse<string>> AssignRolesAsync(AssignRoleRequestDto model)
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
                        response.Success = false;
                        response.Message = "No valid roles were created to assign";
                        return response;
                    }

                    var rolesToAdd = cleanRoles.Except(await _userManager.GetRolesAsync(user)).ToList();

                try
                {
                      var rolesResults = await _userManager.AddToRolesAsync(user,cleanRoles);

                    if (!rolesResults.Succeeded)
                    {
                        response.Success = false;
                        response.Message = string.Join(Environment.NewLine,
                        rolesResults.Errors.Select(e => e.Description));
                        return response;
                    }
                    
                    response.Message = "Roles assign successfully";
                }
                catch(Exception ex)
                {
                    response.Success = false;
                    response.Message = $"Failed to assign roles: {ex.Message}";
                }
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

            
            var key = $"change-password{model.Email}";

            bool blocked = await _rateLimitService.IsRateLimited(key,3,TimeSpan.FromMinutes(15));

            if (blocked) 
            {
                response.Success = false; 
                response.Message = "Too many attempts. Try again after 15 minutes";
                return response; 
            }  

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user is null)
            {
                response.Success = false;
                response.Message = "If a user exists, a change password link has been sent";
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
                    response.Success = false;
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

        public async Task<ServiceResponse<string>> CreateRoleAsync(RolesRequestDto model)
        {
            var response = new ServiceResponse<string>();
            if(model is null){
                response.Success = false;
                response.Message = "Provide valid data to continue";
                return response;
            }

            try
            {
                var results = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
                if (!results.Succeeded)
                {
                    response.Message = string.Join(Environment.NewLine, 
                    results.Errors.Select(e => e.Description));
                    response.Success = false;
                    return response;
                }
                response.Message = "Role created successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to create role: {ex.Message}";
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

            
            var key = $"forgot-password:{model.Email}";

            bool blocked = await _rateLimitService.IsRateLimited(key,3,TimeSpan.FromMinutes(15));

            if (blocked) 
            {
                response.Success = false; 
                response.Message = "Too many attempts. Try again after 15 minutes";
                return response; 
            }  

            var token = string.Empty;
            try
            {
                token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if(!string.IsNullOrWhiteSpace(token))
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

            await _eMailService.ResetPasswordRequestEmailAsync(user.Email!, user.UserName!, token);

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

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if(user is null)
            {
                response.Message = "User not found";
                response.Success = false;
                return response;
            }

            
            var key = $"login:{loginDto.Email}";

            bool blocked = await _rateLimitService.IsRateLimited(key,5,TimeSpan.FromMinutes(1));

            if (blocked) 
            {
                response.Success = false; 
                response.Message = "Too many attempts. Try again after a minute";
                return response; 
            }  
           
           if(await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                try
                {
                    var tokens = await _tokenService.GenerateTokenPairAsync(user);
                    response.Message = "Login successful";
                    response.Data = new AuthenticatedUsertDto
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? string.Empty,
                        AccessToken = tokens.AccessToken,
                        RefreshToken = tokens.RefreshToken,
                        AccessTokenExpiration = tokens.AccessTokenExpiration
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

        public async Task<ServiceResponse<AuthenticatedUsertDto>> RefreshTokenAsync(RefreshRequestDto request)
        {
            var response = new ServiceResponse<AuthenticatedUsertDto>();
            if(request is null || string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
            {
                response.Success = false;
                response.Message = "Provide valid data to continue";
                return response;
            }

            var result = await _tokenService.RefreshTokenAsync(request);

            if(result is null)
            {
                response.Success = false;
                response.Message = "Invalid token refresh request";
                return response;
            }
            response.Data = result;
            response.Message = "Token refreshed successfully";
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

            var userExists = await _userManager.FindByEmailAsync(adminDTO.Email);
            if(userExists is not null)
            {
                response.Success = false;
                response.Message = "The email provided has an account already";
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
                    
                    var uncreatedRoles = new List<string>();
                    foreach (var role in rolesToAdd)
                    {
                        if (!await _roleManager.RoleExistsAsync(role))
                        {
                            uncreatedRoles.Add(role);
                        }
                    }

                    if (uncreatedRoles.Any())
                    {
                        response.Message = "User created but the following roles do not exist and were not assigned: " + string.Join(", ", uncreatedRoles);
                        return response;
                    }

                    var rolesResults = await _userManager.AddToRolesAsync(admin,rolesToAdd);

                    if (!rolesResults.Succeeded)
                    {
                        response.Message = string.Join(Environment.NewLine,
                        rolesResults.Errors.Select(e => e.Description));
                        response.Success = false;
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

            var userExists = await _userManager.FindByEmailAsync(userDTO.Email);
            
            if(userExists is not null)
            {
                response.Success = false;
                response.Message = "A user with the provided email already exists";
                return response;
            }

            var developer = new Developer
            {
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Email = userDTO.Email,
                UserName = userDTO.UserName,
                PhoneNumber = userDTO.PhoneNumber,
                Stack = userDTO.Stack
            };

            try
            {
                var userCreated = await _userManager.CreateAsync(developer,userDTO.Password);
                await _context.SaveChangesAsync();

                if(!userCreated.Succeeded)
                {
                    response.Success = false;
                    response.Message = string.Join(Environment.NewLine,
                    userCreated.Errors.Select(e => e.Description));
                    return response;
                }
                
                response.Data = new RegisterDeveloperResponseDto
                {
                    Id = developer.Id,
                    FirstName = developer.FirstName,
                    LastName = developer.LastName,
                    UserName = developer.UserName,
                    Email = developer.Email,
                    PhoneNumber = developer.PhoneNumber,
                    Stack = developer.Stack
                };

                response.Message = "User created successfully";

                if (userDTO.Roles?.Any() is true)
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
                    
                    var uncreatedRoles = new List<string>();
                    foreach(var role in rolesToAdd)
                    {
                        if(!await _roleManager.RoleExistsAsync(role))
                        {
                            uncreatedRoles.Add(role);
                        }
                    }

                    if (uncreatedRoles.Any())
                    {
                        response.Message = $"User created but the following roles do not exists: {string.Join(Environment.NewLine, uncreatedRoles)}";
                        return response;
                    }
                    
                    var rolesResults = await _userManager.AddToRolesAsync(developer,rolesToAdd);

                    if (!rolesResults.Succeeded)
                    {
                        response.Message =
                            "User created but roles assignment failed: " +
                            string.Join(", ", rolesResults.Errors.Select(e => e.Description));
                            return response;
                    }
                    response.Data.Roles = (await _userManager.GetRolesAsync(developer)).ToList() ?? new List<string>();
                    response.Message = "User created, roles assign successfully";
                }

            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to register new user: {ex.Message}";
            }

            await _eMailService.WelcomeEmailAsync(developer.Email, developer.UserName);
            
            return response;
        }
        public async Task<ServiceResponse<string>> RemoveRoleAsync(string id)
        {
            var response = new ServiceResponse<string>();

            if(id is null || string.IsNullOrEmpty(id)){
                response.Success = false;
                response.Message = "Provide valid data to continue";
                return response;
            }
            try
            {
                var results = await _roleManager.DeleteAsync(new IdentityRole(id));
                if (!results.Succeeded){
                    response.Message = string.Join(Environment.NewLine, 
                    results.Errors.Select(e => e.Description));
                    response.Success = false;

                    return response;
                }
                response.Message = "Role deleted successfully";
            
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"Failed to delete role: {ex.Message}";
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

            await _eMailService.ResetPasswordResponseEmailAsync(user.Email!, user.UserName!);
            return response;
        }
    }
}

        