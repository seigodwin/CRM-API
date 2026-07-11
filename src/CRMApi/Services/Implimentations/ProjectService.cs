
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.ProjectDTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.Services 
{
    public class ProjectService(AppDbContext context, IDistributedRedisCacheService cache) : IProjectService
    {
        private readonly AppDbContext _context = context;
        private readonly IDistributedRedisCacheService _cache = cache;

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
                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();

              
                response.Data = new FullProjectDTO
                {
                    Id = project.Id,
                    Title = project.Title,
                    Description = project.Description,
                    ClientName = project.ClientName,
                    Status = project.Status, 
                    
                    Team = project.Team is null ? null : new FullTeamDTO
                    {
                        Id = project.Team.Id,
                        Title = project.Team.Title,
                        Description = project.Team.Description,
                        TeamLeadId = project.Team.TeamLeadId ?? string.Empty
                    },

                    DateStarted = project.DateStarted,
                    DateUpdated= project.DateUpdated,
                    DateCompleted= project.DateCompleted, 
                };

                response.Message = "Project Created successfully";

               await _cache.SetAsync($"project:{project.Id}", projectDTO,
               TimeSpan.FromMinutes(5),TimeSpan.FromMinutes(10));
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"Database error: {dbEx.Message}";
                response.Success = false;
            }                                                   

            return response;               
        }

        public async Task<ServiceResponse<object>> DeleteProjectById(int id)
        {
            var response = new ServiceResponse<object>(); 

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id); 

            if (project is null) 
            {
                response.Message = $"Project not found!";
                response.Success = false;
                return response;
            }

            try
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                await _cache.RemoveAsync($"project:{id}");
                response.Message = "Project deleted Successfully";
            }

            catch( DbUpdateException dbEx)
            {
                response.Message = $"A database error occured while deleting project: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> DeleteTeam(int projectId, int teamId)
        {

            var response = new ServiceResponse<object>();

            var project = await _context.Projects.Include(p => p.Team)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
            {
                response.Message = $"Project not found!";
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
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 1 : (pageSize > 30 ? 30 : pageSize);
            
            var response = new ServiceResponse<List<FullProjectDTO>>();

            var cacheKey = $"projects:page:{page}:pageSize:{pageSize}";
            var cachedData = await _cache.GetAsync<List<FullProjectDTO>>(cacheKey);
            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Projects retrieved successfully from cache.";
                return response;
            }

            var projectPerPageDTO = new List<FullProjectDTO>();

            var projects = await _context.Projects.AsNoTracking().Include(p => p.Team)
                                                   .OrderBy(p => p.Id)
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
                    TeamId = project.TeamId,
                    DateStarted = project.DateStarted,
                    DateUpdated = project.DateUpdated,
                    DateCompleted = project.DateCompleted,
                    
                    Team = project.Team is null ? null : new FullTeamDTO 
                    { 
                        Id = project.Team.Id,
                        Title = project.Team.Title,
                        Description= project.Team.Description,
                    }
                }); 

                
            }

            response.Data = projectPerPageDTO;
            response.Message = "Projects retrieved successfully" +
                               $" Current Page: {page}" +
                               $" Page Size: {pageSize}" +
                               $" Total Pages {totalPages}";
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<ServiceResponse<FullProjectDTO>> GetProjectById(int id)
        {
            var response = new ServiceResponse<FullProjectDTO>();

            var cacheKey = $"project:{id}";
            var cachedData = await _cache.GetAsync<FullProjectDTO>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Project retrieved successfully from cache.";
                return response;
            }
            
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                response.Message = $"Project not found!";
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

                Team = project.Team is null ? null : new FullTeamDTO
                {
                    Id = project.Team.Id,
                    Title = project.Team.Title,
                    Description = project.Team.Description,
                    TeamLeadId = project.Team.TeamLeadId
                },

                DateStarted = project.DateStarted,
                DateUpdated = project.DateUpdated,
                DateCompleted = project.DateCompleted,
            };
            response.Message = "Project retrieved Successfully";
            
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<ServiceResponse<object>> PatchProjectById(int id, JsonPatchDocument<UpdateProjectRequestDTO> patchData)
        {
            var response = new ServiceResponse<object>();

            if(patchData is null)
            {
                response.Message = "Patch data is null";
                response.Success = false;
                return response;
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project  is null)
            {
                response.Message = $"Project not found";
                response.Success = false;
                return response;
            }

            UpdateProjectRequestDTO projectDTO = new UpdateProjectRequestDTO
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
               await _cache.RemoveAsync($"project:{id}");

               response.Message = "Project updated successfully!";
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"A database error occured while updating project: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<object>> UpdateProjectById(int id, UpdateProjectRequestDTO projectDTO)
        {
            var response = new ServiceResponse<object>();

            if (projectDTO is null)
            {
                response.Message = $"Project data is null!";
                response.Success = false;
                return response;
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project is null)
            {
                response.Message = $"Project not found!";
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
                await _cache.RemoveAsync($"project:{id}");

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
