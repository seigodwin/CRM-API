
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;


namespace CRMApi.Services.Services
{
    public class DeveloperService(AppDbContext context,
    IDistributedRedisCacheService cache, UserManager<ApplicationUser> employeeManager) : IDeveloperService
    {
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

        public async Task<ServiceResponse<List<FullDeveloperDTO>>> GetAllDevelopers(int page = 1, int pageSize = 10)
        {
            var response = new ServiceResponse<List<FullDeveloperDTO>>();

            var cacheKey = $"developers:page:{page}:pageSize:{pageSize}";
            
            var cachedData = await _cache.GetAsync<List<FullDeveloperDTO>>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Developers retrieved successfully from cache.";
                return response;
            }

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

            var developersDTO = developers.Select(d => new FullDeveloperDTO
            {
                Id = d.Id,
                FirstName = d.FirstName,
                SecondName = d.LastName,
                UserName = d.UserName ?? string.Empty,
                Email = d.Email ?? string.Empty,
                PhoneNumber = d.PhoneNumber ?? string.Empty,
                Stack = d.Stack,
                
                Roles = _context.UserRoles
                .Where(ur => ur.UserId == d.Id)
                .Join(
                    _context.Roles, 
                    ur => ur.RoleId, 
                    r => r.Id, 
                    (ur, r) => r.Name
                )// Safely handles the compiler warning if IdentityRole.Name is nullable
                .Select(roleName => roleName ?? string.Empty) 
                .ToList(),
                

                Teams = d.Teams is null ? null : d.Teams.Select(t => new FullTeamDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamLeadId = t.TeamLeadId,
                }).ToList(),

            }).ToList(); 

            response.Data   = developersDTO;
            response.Message = "Developers retrieved successfully " +
                                $"Current Page: {page}" +
                                $" PageSize: {pageSize}" ;

            await _cache.SetAsync(cacheKey, developersDTO, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));   

            return response;     
        }
        public async Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(string id)
        {
            var response = new ServiceResponse<FullDeveloperDTO>();

            var cacheKey = $"developer:{id}";
            var cachedData = await _cache.GetAsync<FullDeveloperDTO>(cacheKey);

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

            response.Data = new FullDeveloperDTO
            {
                Id = developer.Id,
                FirstName = developer.FirstName,
                SecondName = developer.LastName,
                UserName = developer.UserName ?? string.Empty,
                Email = developer.Email ?? string.Empty,
                PhoneNumber = developer.PhoneNumber ?? string.Empty,
                Stack = developer.Stack,
                Roles = (await _employeeManager.GetRolesAsync(developer)).ToList() ?? new List<string>(),

                Teams = developer.Teams is null ? null : developer.Teams.Select(t => new FullTeamDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamLeadId = t.TeamLeadId ?? string.Empty,
                }).ToList(),

            };

            response.Message = "Developer retrieved Successfully";
            await _cache.SetAsync(cacheKey , response.Data, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
            return response;

        }

        public async Task<ServiceResponse<string>> PatchDeveloperById(string id, JsonPatchDocument<PatchDevRequestDTO> patchData)
        {
            var response = new ServiceResponse<string>();

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
                LastName = developer.LastName,
                UserName = developer.UserName ?? string.Empty,
                Email = developer.Email ?? string.Empty,
                Stack = developer.Stack,
            };

            patchData.ApplyTo(developerDTO);
        
            developer.FirstName = developerDTO.FirstName;
            developer.LastName = developerDTO.LastName;
            developer.Email = developerDTO.Email;
            developer.Stack = developerDTO.Stack;

            try
            {
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


        public async Task<ServiceResponse<string>> UpdateDeveloperById(string id, UpdateDevRequestDTO developerDTO)
        {
            var response = new ServiceResponse<string>();

            var developer = await _context.Developers.FindAsync(id);

            if (developer is null)
            {
                response.Message = $"Developer with id: {id} not found!";
                response.Success = false;
                return response;
            }

            developer.FirstName = developerDTO.FirstName;
            developer.LastName = developerDTO.SecondName;
            developer.PhoneNumber = developerDTO.PhoneNumber;
            developer.Email = developerDTO.Email;
            developer.Stack = developerDTO.Stack;

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
