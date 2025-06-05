
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
// Assuming these are your DTOs and models
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using Microsoft.AspNetCore.JsonPatch;
using CRMApi.Utility.Services;



namespace CRMApi.Services.Services
{
    public class DeveloperService(CRMApiDbContext context, BlobServiceClient blobServiceClient) : IDeveloperService
    {
        private readonly CRMApiDbContext _context = context;
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly string _blobContainerName = "photos";
        
        
        public async Task<ServiceResponse<FullDeveloperDTO>> CreateDeveloper(DeveloperDTO developerDTO)
        {
            var response = new ServiceResponse<FullDeveloperDTO>();


            if (developerDTO is null)
            {
                response.Message = "Developer data is null";
                response.Success = false;
                return response;
            }

            if (await _context.Developers.AnyAsync(d => d.Email == developerDTO.Email))
            {
                response.Message = $"Developer with email: {developerDTO.Email} exists";
                response.Success = false;
                return response;
            }

            var developer = new Developer
            {
                Name = developerDTO.Name,
                Contact = developerDTO.Contact,
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

                await _context.Developers.AddAsync(developer);
                await _context.SaveChangesAsync();


                response.Data = new FullDeveloperDTO
                {
                    Id = developer.Id,
                    Name = developer.Name,
                    ImageUrl = developer.ImageUrl,
                    Email = developer.Email,
                    Contact = developer.Contact,
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

        public async Task<ServiceResponse<bool>> DeleteDeveloperById(int id)
        {
            var response = new ServiceResponse<bool>();
             
            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Id == id);
            if (developer == null)
            {
                response.Message = $"Developer with Id {id} not found";
                response.Success = false;
                return response;
            }

            try
            {
                _context.Developers.Remove(developer);
                await _context.SaveChangesAsync();

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

            var developers = await _context.Developers.Include(d => d.Teams)
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

            foreach(var dev in developers)
            {
                developersPerPageDTO.Add(new FullDeveloperDTO 
                { 
                    Id = dev.Id,
                    Name = dev.Name,
                    Contact = dev.Contact,
                    Email = dev.Email,
                    ImageUrl = dev.ImageUrl,
                    Stack = dev.Stack,

                    Teams = dev.Teams is null ? null : dev.Teams.Select(t => new FullTeamDTO
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

            response.Message = "Developers retrieved successfully +" +
                                $"Current Page: {page}" +
                                $"PageSize: {pageSize}" +
                                $"Total Pages: {totalPages}"; 

            return response;
        }
        public async Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(int id)
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
                Name = developer.Name,
                Contact = developer.Contact,
                Email = developer.Email,
                ImageUrl = developer.ImageUrl,
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

        public async Task<ServiceResponse<bool>> PatchDeveloperById(int id, JsonPatchDocument<DeveloperDTO> patchData)
        {
            var response = new ServiceResponse<bool>();

            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Id == id);

            if (developer is null)
            {
                response.Message = $"Developer with Id: {id} not found!";
                response.Success = false;
                return response;
            }


            var developerDTO = new DeveloperDTO
            {
                Name = developer.Name,
                Contact = developer.Contact,
                Email = developer.Email,
                Stack = developer.Stack,
            };


            patchData.ApplyTo(developerDTO);


            developer.Name = developerDTO.Name;
            developer.Contact = developerDTO.Contact;
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


        public async Task<ServiceResponse<bool>> UpdateDeveloperById(int id, DeveloperDTO developerDTO)
        {
            var response = new ServiceResponse<bool>();

            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Id == id);

            if (developer is null)
            {
                response.Message = $"Developer with id: {id} not found!";
                response.Success = false;
                return response;
            }

            developer.Name = developerDTO.Name;
            developer.Contact = developerDTO.Contact;
            developer.Email = developerDTO.Email;
            developer.Stack = developerDTO.Stack;
            
            try
            {
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

        //[Auth]
        //[HttpPost("asign-developer")]
        //public async Task<ServiceResponse<bool>> AsssignDeveloperToTeam(int developerId , int teamId)
        //{

        //}
        
    }
}
