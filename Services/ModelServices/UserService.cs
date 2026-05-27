using Azure.Core;
using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
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
    public class UserService(AppDbContext context,IEmailService emailService,
       UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
       BlobServiceClient blobServiceClient, IJwtTokenGenerator jwtTokenGenerator) : IUserService
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly string _blobContainerName = "photos";
        private readonly IEmailService _emailService = emailService;
        private readonly AppDbContext _context = context;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

        public async Task<ServiceResponse<LoginResponseDTO>> Login(LoginRequestDTO loginDTO)
        {
            var response = new ServiceResponse<LoginResponseDTO>();

        
            if (loginDTO is null)
            {
                response.Message = "Login data is null";
                response.Success = false;
                return response;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email ==  loginDTO.Email);
     
            if (user is not null && !String.IsNullOrEmpty(loginDTO.Password))
            {
                bool isValid = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

                if (isValid)
                {

                    var token = await _jwtTokenGenerator.GenerateTokenAsync(user);

                    response.Data = new LoginResponseDTO
                    {
                        User = new LoggedInUserDTO
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.SecondName,
                            UserName = user.UserName,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                        },

                        Token = token
                    };
                      
                    response.Message = "Login Successful";
                }


            }

            else
            {
                response.Message = "Invalid cridentials. Please provide correct email and password to continue";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> Register(RegistrationRequestDTO userDTO)
        {
            var response = new ServiceResponse<object>(); 

            if(userDTO is null)
            {
                response.Message = "Registeration data is null";
                response.Success = false;
                return response;
            }

            var phoneExists = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userDTO.PhoneNumber);

            if (phoneExists is not null)
            {
                response.Message = $"The phone number {userDTO.PhoneNumber} exist already";
                response.Success = false;
                return response;
            }

            var user = new ApplicationUser
            {
                FirstName = userDTO.FirstName,
                SecondName = userDTO.LastName,
                UserName = userDTO.UserName,
                Email = userDTO.Email,
                PhoneNumber = userDTO.PhoneNumber,
            };

            try 
            {
                if(userDTO.Image is not null && userDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(userDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = userDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob
                    user.ImageUrl = blobClient.Uri.ToString();
                }

                var results = await _userManager.CreateAsync(user, userDTO.Password);

                //Assign role to user upon creating.
                const string ROLENAME = "Admin";

                await _userManager.AddToRoleAsync(user,ROLENAME);

                if (results.Succeeded)
                {
                    response.Message = "User created Successfully";
                }

                else
                {
                    response.Success = false;
                    var errorMessages = results.Errors.Select(e => e.Description); // Select just the description
                    response.Message = $"Registration failed: {string.Join("; ", errorMessages)}"; // Join them with a separator
                }
            
                
            }

            catch (Exception ex)
            {
                response.Message = $"An error occured while creating user: {ex.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<ForgotPasswordResponseDTO>> ForgotPassword(ForgotPasswordRequestDTO model)
        {
            var response = new ServiceResponse<ForgotPasswordResponseDTO>();  

            if (model is null)
            {
                response.Success = false;
                response.Message = "Please provide your email to continue";
                return response;
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                response.Message = $"A password reset message has been sent to {model.Email} if it has an account";
                return response;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // URL encode the token
            var encodedToken = WebUtility.UrlEncode(token);

            
            var resetLink = $"https://projsync.vercel.app//reset-password?email={model.Email}&token={encodedToken}";

            // Compose email
            string subject = "Password Reset Instructions";
            string textBody = $"Dear {user.FirstName},\n\n" +
                              "We received a request to reset your password. " +
                              "Please click the link below to reset it:\n\n" +
                              $"{resetLink}\n\n" +
                              "If you did not request a password reset, please ignore this email.";

            string htmlBody = $@"
    <html>
    <body style='font-family: Arial, sans-serif; color: #333;'>
        <h2>Password Reset Instructions</h2>
        <p>Dear {user.FirstName},</p>
        <p>We received a request to reset your password. Click the button below to reset it:</p>
        <p style='text-align: center;'>
            <a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background-color: #f57c00; color: white; text-decoration: none; border-radius: 5px;'>
                Reset Password
            </a>
        </p>
        <p>If the button doesn’t work, copy and paste the following link into your browser:</p>
        <p style='word-break: break-all;'>
            <a href='{resetLink}'>{resetLink}</a>
        </p>
        <p>If you did not request this, you can safely ignore this email.</p>
        <p>Best regards,<br/>Your App Team</p>
    </body>
    </html>";


            
            bool emailSent = await _emailService.SendEmailAsync(model.Email, subject, textBody, htmlBody);
         
            if (emailSent)
            {
                response.Message = "Password reset instructions sent successfully";
            }

            else
            {
                response.Success = false;
                response.Message = "Failed to send email. Please try again";
            }
                response.Data = new ForgotPasswordResponseDTO
                {
                    Email = model.Email,
                    Token = resetLink
                };

            
            return response;
        }

        public async Task<ServiceResponse<object>> ResetPassword(ResetPasswordDTO model)
        {
            var response = new ServiceResponse<object>();  

            if (model is null)
            {
                response.Success = false;
                response.Message = "Password reset data is empty";   
                return response;
            }
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                response.Message = "Incorrect email. Please try again";
                response.Success = false;
                return response;
            }

            var results = await _userManager.ResetPasswordAsync(user, model.Token,model.NewPassword);

            if (results.Succeeded)
            {
                response.Message = "Password changed successfully";
            }

            else
            {
                response.Message = $"Error: {results.Errors.FirstOrDefault()?.Description}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> AssignRoleAsync(AssignRoleDTO model)
        {
            var response = new ServiceResponse<object>();

            if (model is null)
            {
                response.Success = false;
                response.Message = "Please provide email and role name to continue";
                return response; 
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user is not null)  
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.roleName);
                
                if (roleExists)
                {

                    var userAssigned = await _userManager.AddToRoleAsync(user, model.roleName);

                    
                    if (userAssigned.Succeeded)
                    {
                        response.Message = "Role assigned to user successfully";
                    }

                    else
                    {
                        response.Success = false;
                        response.Message = $"Failed to assign role to user: {userAssigned.Errors?.FirstOrDefault()?.Description}";
                    }
                }

                else
                {
                    response.Success = false;
                    response.Message = $"Rolname {model.roleName} not found";
                }
            }

            else
            {
                response.Success= false;
                response.Message = $"User with email {model.Email} not found!";
            }

            return response;
        }
    }

 

}
