
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;
using CRMApi.Mappings;
using CRMApi.Repository;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;


namespace CRMApi.Services.Services
{
    public class DeveloperService(IBaseRepository<Developer> repo, AppDbContext context,
    IDistributedRedisCacheService cache, UserManager<ApplicationUser> employeeManager) : IDeveloperService
    {
        private readonly IBaseRepository<Developer> _repo = repo;
        private readonly AppDbContext _context = context;
        private readonly IDistributedRedisCacheService _cache = cache;
        private readonly UserManager<ApplicationUser> _employeeManager = employeeManager;
    
        
        public async Task<ServiceResponse<string>> DeleteDeveloperById(string id)
        {
            var response = new ServiceResponse<string>();
             
            var developer = await _employeeManager.FindByIdAsync(id);
            if (developer == null)
            {
                response.Message = $"Developer not found";
                response.Success = false;
                return response;
            }

            try
            {
                await _employeeManager.DeleteAsync(developer);

                await _cache.RemoveAsync($"developer:{id}");

                response.Message = "Developer deleted Successfully";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"An error occured while deleting developer: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<List<GetDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 1 : (pageSize < 30 ? 30 : pageSize);

            var response = new ServiceResponse<List<GetDeveloperDTO>>();

            var cacheKey = $"developers:page:{page}:pageSize:{pageSize}";
            
            var cachedData = await _cache.GetAsync<List<GetDeveloperDTO>>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Developers retrieved successfully from cache.";
                return response;
            }

            var developers = await _repo.GetAllAsync(page, pageSize);

            if (!developers.Any())  
            {
                response.Message = "No records found";
                response.Success = false;
                return response;
            }

            response.Data   = developers.Select(d => d.ToGetDto()).ToList();

            response.Message = "Developers retrieved successfully " +
                                $"Current Page: {page}" +
                                $" PageSize: {pageSize}" ;

            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));   

            return response;
        }
        public async Task<ServiceResponse<GetDeveloperDTO>> GetDeveloperById(string id)
        {
            var response = new ServiceResponse<GetDeveloperDTO>();

            var cacheKey = $"developer:{id}";
            var cachedData = await _cache.GetAsync<GetDeveloperDTO>(cacheKey);

            if(cachedData is not null)
            {
                response.Message = "Data retrieved from cache";
                response.Data = cachedData;
                return response;
            }

            var developer = await _context.Developers.Include(d => d.Teams)
                                               .FirstOrDefaultAsync(d => d.Id == id);
           

            if (developer is null)
            {
                response.Message = $"Developer not found!";
                response.Success = false;
                return response;
            }

           response.Data = developer.ToGetDto();

            response.Message = "Developer retrieved Successfully";
            await _cache.SetAsync(cacheKey , response.Data, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
            return response;

        }

        public async Task<ServiceResponse<string>> PatchDeveloperById(string id, JsonPatchDocument<CreateDeveloperDto> patchData)
        {
            var response = new ServiceResponse<string>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                response.Message = $"Developer not found!";
                response.Success = false;
                return response;
            }

            var developerDTO = developer.ToPatchDto();

            patchData.ApplyTo(developerDTO);

            try
            {
                _context.Developers.Update(developer);
                await _context.SaveChangesAsync();

                await _cache.RemoveAsync($"developer:{id}");

                response.Message = "Developer updated successfully";
            }
            catch (DbUpdateException dbEx)
            {
                response.Message = $"A database error occurred while updating developer: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<string>> UpdateDeveloperById(string id, CreateDeveloperDto developerDTO)
        {
            var response = new ServiceResponse<string>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                response.Message = $"Developer not found!";
                response.Success = false;
                return response;
            }

            developer.Update(developerDTO);

            try
            {
                await _context.SaveChangesAsync();
                await _cache.RemoveAsync($"developer:{id}");
                response.Message = "Developer updated successfully";
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
