
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;
using CRMApi.Exceptions.Types;
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
    IDistributedRedisCacheService cache, ILogger<DeveloperService> logger, UserManager<ApplicationUser> employeeManager) : IDeveloperService
    {
        private readonly IBaseRepository<Developer> _repo = repo;
        private readonly AppDbContext _context = context;
        private readonly IDistributedRedisCacheService _cache = cache;
        private readonly ILogger<DeveloperService> _logger = logger;
        private readonly UserManager<ApplicationUser> _employeeManager = employeeManager;
    
        
        public async Task<ServiceResponse<string>> DeleteDeveloperById(string id)
        {
            var response = new ServiceResponse<string>();
             
            var developer = await _employeeManager.FindByIdAsync(id);

            if(developer is null)
            {
                throw new NotFoundException("Developer not found.");
            }
       
                await _employeeManager.DeleteAsync(developer);

                response.Message = "Developer deleted Successfully";

            try
            {
                await _cache.RemoveAsync($"developer:{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                "Failed to remove developer from cache. {CacheKey}" , $"developer:{id}");
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
                throw new NotFoundException("No developers found.");
            }

            response.Data   = developers.Select(d => d.ToGetDto()).ToList();

            response.Message = "Developers retrieved successfully " +
                                $"Current Page: {page}" +
                                $" PageSize: {pageSize}" ;

            try
            {
                await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache developers data. {CacheKey}", cacheKey);
            }

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
               throw new NotFoundException("Developer not found.");
            }

           response.Data = developer.ToGetDto();

            response.Message = "Developer retrieved Successfully";

            try
            {
                await _cache.SetAsync(cacheKey , response.Data, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache developer data. {CacheKey}", cacheKey);
            }

            return response;

        }

        public async Task<ServiceResponse<string>> PatchDeveloperById(string id, JsonPatchDocument<CreateDeveloperDto> patchData)
        {
            var response = new ServiceResponse<string>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
               throw new NotFoundException("Developer not found.");
            }

            var developerDTO = developer.ToPatchDto();

            patchData.ApplyTo(developerDTO);
            
            _context.Developers.Update(developer);
            await _context.SaveChangesAsync();
            response.Message = "Developer updated successfully";
                 
            try
            {
                await _cache.RemoveAsync($"developer:{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove developer from cache. {CacheKey}", $"developer:{id}");
            }

            return response;
        }

        public async Task<ServiceResponse<string>> UpdateDeveloperById(string id, CreateDeveloperDto developerDTO)
        {
            var response = new ServiceResponse<string>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                throw new NotFoundException("Developer not found.");
            }

            developer.Update(developerDTO);
            await _context.SaveChangesAsync();
            response.Message = "Developer updated successfully";

            try
            {
                await _cache.RemoveAsync($"developer:{id}");
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove developer from cache. {CacheKey}", $"developer:{id}");
            }

            return response;
        }

    }
}
