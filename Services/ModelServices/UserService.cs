using System.IdentityModel.Tokens.Jwt;  
using System.Security.Claims;
using System.Text;
using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static System.Net.WebRequestMethods;

namespace CRMApi.Services.Services
{
    public class UserService(CRMApiDbContext context, IConfiguration config, IEmailService emailService, BlobServiceClient blobServiceClient) : IUserService
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly string _blobContainerName = "photos";
        private readonly IEmailService _emailService = emailService;
        private readonly CRMApiDbContext _context = context;
        private readonly IConfiguration _config = config;


        [HttpPost("login")]
        public async Task<ServiceResponse<string>> Login(LoginDTO loginDTO)
        {
            var response = new ServiceResponse<string>();

            if (loginDTO is null)
            {
                response.Message = "Login cridentials is null";
                response.Success = false;
                return response;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
            {
                response.Message = "Invalid Cridentials";
                response.Success = false;
                return response;
            }

            response.Data = GenerateJwtToken(user);
            response.Message = "Login Successful";

            return response;
        }


        [HttpPost("sign-up")]
        public async Task<ServiceResponse<bool>> Register(UserDTO userDTO)
        {
            var response = new ServiceResponse<bool>();

            if (userDTO is null)
            {
                response.Message = "Sign Up cridentials is null";
                response.Success = false;
                return response;
            }

            if(await _context.Users.AnyAsync(u => u.Email == userDTO.Email))
            {
                response.Message = $"User with email {userDTO.Email} exists already";
                response.Success = false;
                return response;
            }

            var user = new User
            {
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Email = userDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userDTO.Password)
            };

            try
            {
                if (userDTO.Image != null && userDTO.Image.Length > 0)
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

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                response.Message = "User created Successfully";
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"A database error occured while adding new User: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        [HttpPost("reset-password")]
        public async Task<ServiceResponse<bool>> ResetPassword(string email, string? otp = null, string? newPassword = null, string? confirmPassword = null)
        {
            var response = new ServiceResponse<bool>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
             
            if (user == null)
            {
                response.Message = "User not found";
                response.Success = false;
                return response;
            }

            
            if (string.IsNullOrEmpty(otp))
            {
                var generatedOTP = GenerateOTP();
                user.OTP = generatedOTP;
                user.OTPExpiration = DateTime.UtcNow.AddMinutes(5); 

                await _context.SaveChangesAsync();

               
                string subject = "Password Reset OTP";
                string body = $"Your OTP for password reset is: {generatedOTP}. This OTP expires in 5 minutes.";

                bool emailSent = await _emailService.SendEmail(email, subject, body);
                if (!emailSent)
                {
                    response.Message = "Failed to send OTP. Please try again.";
                    response.Success = false;
                    return response;
                } 

                response.Message = "OTP sent successfully.";
                return response;
            }

           
            if (user.OTP != otp || user.OTPExpiration < DateTime.UtcNow)
            {
                response.Message = "Invalid or expired OTP";
                response.Success = false;
                return response;
            }

            
            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                response.Message = "Passwords do not match or are invalid";
                response.Success = false;
                return response;
            }

            
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.OTP = null;
            user.OTPExpiration = null;

            await _context.SaveChangesAsync();

            response.Message = "Password reset successfully";
            return response;
        }

 

        private string GenerateOTP()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }


        private async string GenerateJwtToken(User user)
        {
            var KeyVaultUrl = _config["KeyVault:KeyVaultUrl"];
            var KeyVaultClient = new SecretClient(new Uri(KeyVaultUrl), new DefaultAzureCridential());

            var JwtSecret = await KeyVaultClient.GetSecretAsync("JwtKey");
            string JwtSecretValue = JwtSecret.Value.Value;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretValue));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
