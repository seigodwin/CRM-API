
using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using CRMApi.Utility.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;



namespace CRMApi.Services.Services
{
    public class DeveloperService(AppDbContext context, BlobServiceClient blobServiceClient, 
        UserManager<ApplicationUser> employeeManager, IJwtTokenGenerator tokenGenerator) : IDeveloperService
    {
        private readonly AppDbContext _context = context;
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly string _blobContainerName = "photos";
        private readonly UserManager<ApplicationUser> _employeeManager = employeeManager;
        private readonly IJwtTokenGenerator _tokenGenerator = tokenGenerator;
        
        
        public async Task<ServiceResponse<FullDeveloperDTO>> CreateDeveloper(DevRegistrationRequestDTO developerDTO)
        {
            var response = new ServiceResponse<FullDeveloperDTO>();


            if (developerDTO is null)
            {
                response.Message = "Developer data is null";
                response.Success = false;
                return response;
            }

            if (await _employeeManager.FindByEmailAsync(developerDTO.Email) is not null)
            {
                response.Message = $"Developer with email: {developerDTO.Email} exists";
                response.Success = false;
                return response;
            }

            if (await _context.Users.FirstOrDefaultAsync(d => d.PhoneNumber == developerDTO.PhoneNumber) is not null)
            {
                response.Message = $"Developer with phone number: {developerDTO.PhoneNumber} exists";
                response.Success = false;
                return response;
            }

            var developer = new Developer
            {
                FirstName = developerDTO.FirstName,
                SecondName = developerDTO.SecondName, 
                PhoneNumber = developerDTO.PhoneNumber,
                UserName = developerDTO.UserName ?? developerDTO.Email,
                Email = developerDTO.Email,
                Stack = developerDTO.Stack,
            };

            try
            {
                if (developerDTO.Image != null && developerDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(developerDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = developerDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob
                    developer.ImageUrl = blobClient.Uri.ToString();
                }

                await _employeeManager.CreateAsync(developer, developerDTO.Password);
                await _context.SaveChangesAsync();

                const string ROLENAME = "Employee";

                await _employeeManager.AddToRoleAsync(developer, ROLENAME);

                response.Data = new FullDeveloperDTO
                {
                    Id = developer.Id,
                    FirstName = developer.FirstName,
                    SecondName= developer.SecondName,
                    UserName = developer.UserName,  
                    ImageUrl = developer.ImageUrl,
                    Email = developer.Email,
                    PhoneNumber = developer.PhoneNumber,
                    Stack = developer.Stack,
                };
                response.Message = "Developer Created Successfully";

            }
            catch (DbUpdateException dbEx)
            {
                response.Message = $"Database error: {dbEx.Message}";
                response.Success = false;
            }

            catch (Exception ex)
            {
                response.Message = $"An error occured while creating developer : {ex.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> DeleteDeveloperById(string id)
        {
            var response = new ServiceResponse<object>();
             
            var developer = await _employeeManager.FindByIdAsync(id);
            if (developer == null)
            {
                response.Message = $"Developer with Id {id} not found";
                response.Success = false;
                return response;
            }

            try
            {
                await _employeeManager.DeleteAsync(developer);

                response.Message = "Developer deleted Successfully";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"An error occured while deleting developer: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<List<FullDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10)
        {
            var response = new ServiceResponse<List<FullDeveloperDTO>>();

            var employeeRoleId = await _context.Roles.Where(r => r.Name == "Employee")
                                                     .Select(r => r.Id)
                                                     .FirstOrDefaultAsync();

            var developers = await _context.Developers.Include(d => d.Teams)
                                                       .Where(u => _context.UserRoles
                                                       .Any(ur => ur.UserId == u.Id && ur.RoleId == employeeRoleId))
                                                       .Skip((page - 1) * pageSize) 
                                                       .Take(pageSize) 
                                                       .ToListAsync(); 


            if (developers.Count == 0)  
            {
                response.Message = "No records found";
                response.Success = false;
                return response;
            }

            int totalDevelopers = developers.Count;
            var totalPages = (int)Math.Ceiling((decimal)totalDevelopers / pageSize);

            var developersPerPageDTO = new List<FullDeveloperDTO>();

            foreach(var developer in developers)
            {
                developersPerPageDTO.Add(new FullDeveloperDTO 
                {
                    Id = developer.Id,
                    FirstName = developer.FirstName,
                    SecondName = developer.SecondName,
                    UserName = developer.UserName,
                    ImageUrl = developer.ImageUrl,
                    Email = developer.Email,
                    PhoneNumber = developer.PhoneNumber,
                    Stack = developer.Stack,

                    Teams = developer.Teams is null ? null : developer.Teams.Select(t => new FullTeamDTO
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        TeamLeadId = t.TeamLeadId,
                    }).ToList(),

                });
            }
            response.Data = developersPerPageDTO;
            ;

            response.Message = "Developers retrieved successfully " +
                                $"Current Page: {page}" +
                                $" PageSize: {pageSize}" +
                                $" Total Pages: {totalPages}"; 

            return response;     
        }
        public async Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(string id)
        {
            var response = new ServiceResponse<FullDeveloperDTO>();

            var developer = await _context.Developers.Include(d => d.Teams)
                                                     .FirstOrDefaultAsync(d => d.Id == id);

            if (developer is null)
            {
                response.Message = $"Developer with id {id} not found!";
                response.Success = false;
                return response;
            }

            response.Data = new FullDeveloperDTO
            {
                Id = developer.Id,
                FirstName = developer.FirstName,
                SecondName = developer.SecondName,
                UserName = developer.UserName,
                ImageUrl = developer.ImageUrl,
                Email = developer.Email,
                PhoneNumber = developer.PhoneNumber,
                Stack = developer.Stack,

                Teams = developer.Teams is null ? null : developer.Teams.Select(t => new FullTeamDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamLeadId = t.TeamLeadId,
                }).ToList(),

            };

            response.Message = "Developer retrieved Successfully";

            return response;

        }

        public async Task<ServiceResponse<LoginResponseDTO>> Login(DeveloperLoginRequestDTO model)
        {
            var response = new ServiceResponse<LoginResponseDTO>();

            if(model is null)
            {
                response.Success = false;
                response.Message = "Please provide login cridentials to continue";
                return response;
            }

            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Email == model.Email);
            if (developer is not null && !String.IsNullOrEmpty(model.Password))
            {
                bool isValid = await _employeeManager.CheckPasswordAsync(developer, model.Password);
                if (isValid)
                {

                    response.Message = "Login successful";
                    var token = await _tokenGenerator.GenerateTokenAsync(developer);
                    
                    response.Data = new LoginResponseDTO
                        {
                        User = new LoggedInUserDTO
                        {
                            Id = developer.Id,
                            FirstName = developer.FirstName,
                            LastName = developer.SecondName,
                            UserName = developer.UserName,
                            Email = developer.Email,
                            PhoneNumber = developer.PhoneNumber,
                        },

                        Token = token
                        };
                    ;
                }
                else
                {
                    response.Message = "Incorrect Password";
                    response.Success = false;
                }
            }
            else
            {
                response.Message = "Incorrect Login cridentials";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> PatchDeveloperById(string id, JsonPatchDocument<PatchDevRequestDTO> patchData)
        {
            var response = new ServiceResponse<object>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                response.Message = $"Developer with Id: {id} not found!";
                response.Success = false;
                return response;
            }


            var developerDTO = new PatchDevRequestDTO
            {
                FirstName = developer.FirstName,
                SecondName = developer.SecondName,
                UserName = developer.UserName,  
                PhoneNumber = developer.PhoneNumber,
                Email = developer.Email,
                Stack = developer.Stack,
            };


            patchData.ApplyTo(developerDTO);
            

            developer.FirstName = developerDTO.FirstName;
            developer.SecondName = developerDTO.SecondName;
            developer.Email = developerDTO.Email;
            developer.Stack = developerDTO.Stack;

            try
            {
                await _context.SaveChangesAsync();
                response.Message = "Developer updated successfully";
            }
            catch (DbUpdateException dbEx)
            {
                response.Message = $"A database error occurred while updating developer: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }


        public async Task<ServiceResponse<object>> UpdateDeveloperById(string id, UpdateDevRequestDTO developerDTO)
        {
            var response = new ServiceResponse<object>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                response.Message = $"Developer with id: {id} not found!";
                response.Success = false;
                return response;
            }

            developer.FirstName = developerDTO.FirstName;
            developer.SecondName = developerDTO.SecondName;
            developer.PhoneNumber = developerDTO.PhoneNumber;
            developer.Email = developerDTO.Email;
            developer.Stack = developerDTO.Stack;

            
            try
            { 
                if(developerDTO.Image is not null && developerDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(developerDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = developerDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob
                    developer.ImageUrl = blobClient.Uri.ToString();
                }

                await _context.SaveChangesAsync();

                response.Message = "Developer Updated Successfully";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"Database error: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

    }
}
