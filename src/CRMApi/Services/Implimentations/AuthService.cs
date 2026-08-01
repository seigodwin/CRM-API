
using CRM_API.Domain.DTos;
using CRM_API.Domain.DTos.AuthDtos;
using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.AuthDtos;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;
using CRMApi.Exceptions.Types;
using CRMApi.Mappings;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


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
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context,UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager, IRateLimitService rateLimitService,
        ITokenService tokenService, IEmailService emailService , ILogger<AuthService> logger)
        {
            _rateLimitService = rateLimitService;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
            _eMailService = emailService;
            _logger = logger;
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
                throw new NotFoundException("User not found.");
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
                throw new AuthenticationException("Too many attempts. Try again after 15 minutes");
            }  

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user is null)
            {
                throw new NotFoundException("If a user exists, a change password link has been sent");
            }

                var results = await _userManager
                .ChangePasswordAsync(user,model.CurrentPassword,model.NewPassword);

                if (!results.Succeeded)
                {
                    throw new BadRequestException(string.Join(Environment.NewLine,
                    results.Errors.Select( e => e.Description)));
                }
                response.Message = "Password change successfully";
 
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
               throw new NotFoundException("User not found.");
            }

            try
            {
                var results = await _userManager.ConfirmEmailAsync(user,model.Token);
                if (!results.Succeeded)
                {
                    throw new ValidationsException(results.Errors.Select(e => e.Description));
                }
                response.Message = "Email Confirmed successfully";
            }

            catch(Exception ex)
            {
                throw new BadRequestException($"Failed to verify email: {ex.Message}");
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
                throw new BadRequestException($"Failed to create role: {ex.Message}");
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
    
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if(!string.IsNullOrWhiteSpace(token))
                {
                    response.Data = token;
                    response.Message = "Password reset token generated";
                }

            try
            {
                await _eMailService.ResetPasswordRequestEmailAsync(user.Email!, user.UserName!, token);
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
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

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if(user is null)
            {
                throw new NotFoundException("Incorrect email or password");
            }
            
            var key = $"login:{loginDto.Email}";

            bool blocked = await _rateLimitService.IsRateLimited(key,5,TimeSpan.FromMinutes(1));

            if (blocked) 
            {
                throw new AuthenticationException("Too many attempts. Try again after 1 minute");
            }  
           
           if(await _userManager.CheckPasswordAsync(user, loginDto.Password))
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

            else
            {
                throw new AuthenticationException("Incorrect email or password");
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
                throw new AuthenticationException("Invalid token or refresh token");
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
                throw new ConflictException("A user with the provided email already exists");
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

        public async Task<ServiceResponse<GetDeveloperDTO>> RegisterDeveloperAsync(CreateDeveloperDto userDTO)
        {
            var response = new ServiceResponse<GetDeveloperDTO>();
            if(userDTO is null)
            {
                response.Success = false;
                response.Message = "Provide registeration data to continue";
                return response;
            }

            
            if(await _userManager.FindByEmailAsync(userDTO.Email) is not null)
            {
                throw new ConflictException("A user with the provided email already exists");
            }

            var developer = userDTO.ToEntity();
           
              var userCreated = await _userManager.CreateAsync(developer,userDTO.Password);
                await _context.SaveChangesAsync();

                if(!userCreated.Succeeded)
                {
                    throw new ValidationsException(userCreated.Errors.Select(e => e.Description));
                }
                
                response.Data = developer.ToGetDto();

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

            try
            {
                await _eMailService.WelcomeEmailAsync(developer.Email!, developer.UserName!);

            }        
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", developer.Email);
            }

            return response;
        }

        public async Task<ServiceResponse<string>> RemoveRoleAsync(string id)
        {
            var response = new ServiceResponse<string>();

            if(id is null || string.IsNullOrEmpty(id)){
                response.Success = false;
                response.Message = "Invalid user id";
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
            catch(Exception)
            {
                response.Success = false;
                response.Message = $"Failed to delete role";
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

                var changedPassword = await _userManager.ResetPasswordAsync(user,model.Token,model.NewPassword);
                if (!changedPassword.Succeeded)
                {
                    throw new AuthenticationException(string.Join(", ", changedPassword.Errors.Select(e => e.Description)));
                }

                response.Message = "Password reset success";                
            

            await _eMailService.ResetPasswordResponseEmailAsync(user.Email!, user.UserName!);
            return response;
        }
    }
}
