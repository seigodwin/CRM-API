using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.Services
{
    public class ProjectService(CRMApiDbContext context, BlobServiceClient blobServiceClient) : IProjectService
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly string _blobContainerName = "photos";
        private readonly CRMApiDbContext _context = context;

        public async Task<ServiceResponse<FullProjectDTO>> CreateProject(ProjectDTO projectDTO)
        {
            var response = new ServiceResponse<FullProjectDTO>();

            if (projectDTO is null) 
            {
                response.Message = "Project DTO is null!";
                response.Success = false;
                return response;
            }

            var project = new Project
            {
                Title = projectDTO.Title,
                Description = projectDTO.Description,
                ClientName = projectDTO.ClientName,
                Status = (ProjectStatus)projectDTO.Status,
                DateStarted = projectDTO.DateStarted,
                TeamId = projectDTO.TeamId,
            };

            try
            {
                if (projectDTO.Image != null && projectDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(projectDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = projectDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob
                    project.ImageUrl = blobClient.Uri.ToString();  
                    
                }

                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();

                response.Data = new FullProjectDTO
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    ClientName = project.ClientName,
                    Status = project.Status, 
                    
                    Team = new FullTeamDTO
                    {
                        Id = project.Team.Id,
                        Title = project.Team.Title,
                        Description = project.Team.Description,
                        TeamLeadId = project.Team.TeamLeadId
                    },
                    DateStarted = project.DateStarted,
                    DateUpdated= project.DateUpdated,
                    DateCompleted= project.DateCompleted, 
                    ImageUrl = project.ImageUrl
                };

                response.Message = "Project Created successfully";

            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"Database error: {dbEx.Message}";
                response.Success = false;

            }                                                   

            return response;               
        }

        public async Task<ServiceResponse<bool>> DeleteProjectById(int id)
        {
            var response = new ServiceResponse<bool>(); 

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id); 

            if (project is null) 
            {
                response.Message = $"Project with Id: {id} not found!";
                response.Success = false;
                return response;
            }

            try
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                response.Message = "Project deleted Successfully";
            }

            catch( DbUpdateException dbEx)
            {
                response.Message = $"A database error occured while deleting project: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteTeam(int projectId, int teamId)
        {

            var response = new ServiceResponse<bool>();

            var project = await _context.Projects.Include(p => p.Team)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
            {
                response.Message = $"Project with Id: {projectId} not found!";
                response.Success = false;
                return response;
            }

            if (project.Team is not null)
            {
                project.TeamId = null;

                try
                {
                    await _context.SaveChangesAsync();
                    response.Message = "Team deleted successfully";
                }

                catch ( DbUpdateException dbEx)
                {
                    response.Message = $"Database error: {dbEx.Message}";
                    response.Success = false;
                }
            }

            return response;

        }
        public async Task<ServiceResponse<List<FullProjectDTO>>> GetAllProjects(int page = 1, int pageSize = 10)
        {
            var response = new ServiceResponse<List<FullProjectDTO>>();

            var projectPerPageDTO = new List<FullProjectDTO>();

            var projects = await _context.Projects.Include(p => p.Team)
                                                   .Skip((page - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToListAsync();

            if (projects.Count == 0)
            {
                response.Message = "No records found!";
                response.Success = false;
                return response;
            }

            var totalProjects = projects.Count;
            var totalPages = (int)Math.Ceiling((decimal)totalProjects / pageSize);


            foreach (var project in projects)
            {
                projectPerPageDTO.Add(new FullProjectDTO
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    ClientName = project.ClientName,
                    Status = project.Status,

                    Team = new FullTeamDTO
                    {
                        Id = project.Team.Id,
                        Title = project.Team.Title,
                        Description = project.Team.Description,
                        TeamLeadId = project.Team.TeamLeadId
                    },

                    TeamId = project.TeamId,
                    DateStarted = project.DateStarted,
                    DateUpdated = project.DateUpdated,
                    DateCompleted = project.DateCompleted,
                    ImageUrl = project.ImageUrl,
                });
            }

            response.Data = projectPerPageDTO;
            response.Message = "Projects retrieved successfully" +
                               $"Current Page: {page}" +
                               $"Page Size: {pageSize}" +
                               $"Total Pages {totalPages}";

            return response;
        }

        public async Task<ServiceResponse<FullProjectDTO>> GetProjectById(int id)
        {
            var response = new ServiceResponse<FullProjectDTO>();

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                response.Message = $"Project with id: {id} not found!";
                response.Success = false;
                return response; 
            }

            response.Data = new FullProjectDTO
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                ClientName = project.ClientName,
                Status = project.Status,

                Team = new FullTeamDTO
                {
                    Id = project.Team.Id,
                    Title = project.Team.Title,
                    Description = project.Team.Description,
                    TeamLeadId = project.Team.TeamLeadId
                },

                DateStarted = project.DateStarted,
                DateUpdated = project.DateUpdated,
                DateCompleted = project.DateCompleted,
                ImageUrl = project.ImageUrl,
            };
            response.Message = "Project retrieved Successfully";

            return response;
        }

        public async Task<ServiceResponse<bool>> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData)
        {
            var response = new ServiceResponse<bool>();

            if(patchData is null)
            {
                response.Message = "Patch data is null";
                response.Success = false;
                return response;
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project  is null)
            {
                response.Message = $"Project with Id: {id} not found";
                response.Success = false;
                return response;
            }

            ProjectDTO projectDTO = new ProjectDTO
            {
                Title = project.Title,
                Description = project.Description,
                ClientName = project.ClientName,
                Status = project.Status,
                TeamId = project.TeamId,
                DateStarted = project.DateStarted,
                DateUpdated = project.DateUpdated,
                DateCompleted = project.DateCompleted,
            };

            patchData.ApplyTo(projectDTO);

            project.Title = projectDTO.Title;
            project.Description = projectDTO.Description;
            project.ClientName = projectDTO.ClientName;
            project.Status = (ProjectStatus)projectDTO.Status;
            project.TeamId = projectDTO.TeamId;
            project.DateStarted = projectDTO.DateStarted;
            project.DateUpdated = projectDTO.DateUpdated;
            project.DateCompleted = projectDTO.DateCompleted;

            try
            {
               await _context.SaveChangesAsync();

               response.Message = "Project updated successfully!";
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"A database error occured while updating project: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> UpdateProjectById(int id, ProjectDTO projectDTO)
        {
            var response = new ServiceResponse<bool>();

            if (projectDTO is null)
            {
                response.Message = $"Project data is null!";
                response.Success = false;
                return response;
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                response.Message = $"Project with id: {id} not found!";
                response.Success = false;
                return response;
            }

            project.Title = projectDTO.Title;
            project.Description = projectDTO.Description;
            project.ClientName = projectDTO.ClientName;
            project.Status = (ProjectStatus)projectDTO.Status;
            project.TeamId = projectDTO.TeamId;
            project.DateStarted = projectDTO.DateStarted;
            project.DateUpdated = projectDTO.DateUpdated;
            project.DateCompleted = projectDTO.DateCompleted;

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
    }
}
