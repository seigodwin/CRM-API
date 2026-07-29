
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
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
    AppDbContext context,  IDistributedRedisCacheService cache) : IProjectService
    {
        private readonly IBaseRepository<Project> _repo = repo;
        private readonly AppDbContext _context = context;
        private readonly IDistributedRedisCacheService _cache = cache;

        public async Task<ServiceResponse<GetProjectDto>> CreateProject(ProjectDTO projectDTO)
        {
            var response = new ServiceResponse<GetProjectDto>();

            if (projectDTO is null) 
            {
                response.Message = "Project DTO is null!";
                response.Success = false;
                return response;
            }

            var project = projectDTO.ToEntity();

            try
            {
                await _repo.AddAsync(project);
              
                response.Data = project.ToGetDto();

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

            var project = await _repo.GetById(id); 

            if (project is null) 
            {
                response.Message = $"Project not found!";
                response.Success = false;
                return response;
            }

            try
            {
                await _repo.DeleteAsync(project);

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
                response.Message = "No records found!";
                response.Success = false;
                return response;
            }

            response.Data = projects.Select(p => p.ToGetDto()).ToList();

            response.Message = "Projects retrieved successfully" +
                               $" Current Page: {page}" +
                               $" Page Size: {pageSize}";
                              
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
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
                response.Message = $"Project not found!";
                response.Success = false;
                return response; 
            }

            response.Data = project.ToGetDto();
            
            response.Message = "Project retrieved Successfully";
            
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
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
                response.Message = $"Project not found";
                response.Success = false;
                return response;
            }

            var dto = project.ToPatchDto();

            patchData.ApplyTo(dto);

            try
            {
               await _repo.UpdateAsync(project);
             
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
                response.Message = $"Project not found!";
                response.Success = false;
                return response;
            }

            project.Update(projectDTO);

            try
            {
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
