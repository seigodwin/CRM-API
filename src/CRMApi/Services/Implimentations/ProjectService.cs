
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Exceptions.Types;
using CRMApi.Extentions;
using CRMApi.Mappings;
using CRMApi.Repository;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.Services 
{
    public class ProjectService(IBaseRepository<Project> repo,
    AppDbContext context,  IDistributedRedisCacheService cache,
    ILogger<ProjectService> logger) : IProjectService
    {
        private readonly IBaseRepository<Project> _repo = repo;
        private readonly AppDbContext _context = context;
        private readonly IDistributedRedisCacheService _cache = cache;
        private readonly ILogger<ProjectService> _logger = logger;

        public async Task<ServiceResponse<GetProjectDto>> CreateProject(ProjectDTO projectDTO)
        {
            var response = new ServiceResponse<GetProjectDto>();

            if (projectDTO is null) 
            {
                throw new NotFoundException("Project data is null!");
            }

            var project = projectDTO.ToEntity();

            await _repo.AddAsync(project);
              
            response.Data = project.ToGetDto();

            response.Message = "Project Created successfully";

            try
            {
               await _cache.SetAsync($"project:{project.Id}", projectDTO,
               TimeSpan.FromMinutes(5),TimeSpan.FromMinutes(10));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache project data. {CacheKey}", $"project:{project.Id}");
            }   
                                                   
            return response;               
        }

        public async Task<ServiceResponse<object>> DeleteProjectById(int id)
        {
            var response = new ServiceResponse<object>(); 

            var project = await _repo.GetById(id); 

            if (project is null) 
            {
                throw new NotFoundException("Project not found!");
            }

            await _repo.DeleteAsync(project);
            response.Message = "Project deleted Successfully";

            try
            {
                await _cache.RemoveAsync($"project:{id}");
            }

            catch( Exception ex)
            {
                _logger.LogError(ex, "Failed to remove project from cache. {CacheKey}", $"project:{id}");
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
                throw new NotFoundException("Project not found!");
            }

            if (project.Team is not null)
            {
                project.TeamId = null;

                await _context.SaveChangesAsync();
                response.Message = "Team deleted successfully";
            }
            
            return response;

        }
        public async Task<ServiceResponse<List<GetProjectDto>>> GetAllProjects(int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 1 : (pageSize > 30 ? 30 : pageSize);
            
            var response = new ServiceResponse<List<GetProjectDto>>();

            var cacheKey = $"projects:page:{page}:pageSize:{pageSize}";
            var cachedData = await _cache.GetAsync<List<GetProjectDto>>(cacheKey);
            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Projects retrieved successfully from cache.";
                return response;
            }

            var projects = await _repo.GetAllAsync(page, pageSize);

            if (!projects.Any())
            {
                throw new NotFoundException("No projects found.");
            }

            response.Data = projects.Select(p => p.ToGetDto()).ToList();

            response.Message = "Projects retrieved successfully" +
                               $" Current Page: {page}" +
                               $" Page Size: {pageSize}";

            try
            {
                await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache projects data. {CacheKey}", cacheKey);
            }

            return response;
        }

        public async Task<ServiceResponse<GetProjectDto>> GetProjectById(int id)
        {
            var response = new ServiceResponse<GetProjectDto>();

            var cacheKey = $"project:{id}";
            var cachedData = await _cache.GetAsync<GetProjectDto>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Project retrieved successfully from cache.";
                return response;
            }
            
            var project = await _repo.GetById(id);

            if (project is null)
            {
                throw new NotFoundException("Project not found!");
            }

            response.Data = project.ToGetDto();
            
            response.Message = "Project retrieved Successfully";

            try
            {
                await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache project data. {CacheKey}", cacheKey);
            }

            return response;
        }

        public async Task<ServiceResponse<object>> PatchProjectById(int id, JsonPatchDocument<ProjectDTO> patchData)
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
                throw new NotFoundException("Project not found!");
            }

            var dto = project.ToPatchDto();

            patchData.ApplyTo(dto);

            await _repo.UpdateAsync(project);
            response.Message = "Project updated successfully!";

            try
            {
               await _cache.RemoveAsync($"project:{id}");
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to remove project from cache. {CacheKey}", $"project:{id}");
            }

            return response;
        }

        public async Task<ServiceResponse<object>> UpdateProjectById(int id, ProjectDTO projectDTO)
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
                throw new NotFoundException("Project not found!");
            }

            project.Update(projectDTO);
            response.Message = "Project Updated Successfully";

            try
            {
                await _cache.RemoveAsync($"project:{id}");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove project from cache. {CacheKey}", $"project:{id}");
            }   
    
            return response;
        }
    }
}
