
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
    public class DeveloperService(AppDbContext context, UserManager<ApplicationUser> employeeManager, IJwtTokenGenerator tokenGenerator) : IDeveloperService
    {
        private readonly AppDbContext _context = context;
        private readonly UserManager<ApplicationUser> _employeeManager = employeeManager;
        private readonly IJwtTokenGenerator _tokenGenerator = tokenGenerator;
        
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

           response.Data = developers.Select(d => new FullDeveloperDTO
            {
                Id = d.Id,
                FirstName = d.FirstName,
                SecondName = d.LastName,
                UserName = d.UserName!,
                Email = d.Email!,
                PhoneNumber = d.PhoneNumber,
                Stack = d.Stack,

                Teams = d.Teams is null ? null : d.Teams.Select(t => new FullTeamDTO
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TeamLeadId = t.TeamLeadId,
                }).ToList(),

            }).ToList(); 

            response.Message = "Developers retrieved successfully " +
                                $"Current Page: {page}" +
                                $" PageSize: {pageSize}" ;
                                

            return response;     
        }
        public async Task<ServiceResponse<FullDeveloperDTO>> GetDeveloperById(string id)
        {
            var response = new ServiceResponse<FullDeveloperDTO>();

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
                UserName = developer.UserName!,
                Email = developer.Email!,
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
                UserName = developer.UserName!,  
                Email = developer.Email!,
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
