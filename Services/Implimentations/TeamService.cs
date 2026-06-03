
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.ModelServices
{
    public class TeamService(AppDbContext context, IRedisCacheService cache) : ITeamService
    {
        private readonly AppDbContext _context = context;
        private readonly IRedisCacheService _cache = cache;

        public async Task<ServiceResponse<object>> AssignDeveloperToTeam(string DeveloperId, int TeamId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Developers)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";
                response.Success = false;
                return response;
            }

            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Id == DeveloperId);

            if (developer is null)
            {
                response.Message = $"Developer with Id {DeveloperId} not found!";
                response.Success = false;
                return response;
            }

            if (team.Developers.Count is not 0)
            {
                if (team.Developers.Any(d => d.Id == DeveloperId))
                {
                    response.Message = $"This developer is already a member of this Team";
                    response.Success = false;
                    return response;
                }
            }

            try
            {
                team.Developers.Add(developer);
                await  _context.SaveChangesAsync();
                response.Message = "Developer added to the Team successfully";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"A database error occured: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }
         

        public async Task<ServiceResponse<object>> AssignProjectToTeam(int ProjectId, int TeamId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Projects)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";
                response.Success = false;
                return response;
            }

            var project = await _context.Projects.FirstOrDefaultAsync(d => d.Id == ProjectId);

            if (project is null)
            {
                response.Message = $"Project with Id {ProjectId} not found!";
                response.Success = false;
                return response;
            }

            if (team.Projects is not null && team.Projects.Count is not 0)
            {
                if (team.Projects.Any(p => p.Id == ProjectId))
                {
                    response.Message = $"Project with Id {ProjectId} exists in Team with Id {TeamId} already";
                    response.Success = false;
                    return response;
                }
            }

            try
            {
                team.Projects?.Add(project);
                await _context.SaveChangesAsync();
                response.Message = "Project assigned to the Team successfully";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"A database error occured: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }

        public async Task<ServiceResponse<FullTeamDTO>> CreateTeam(TeamDTO teamDTO)
        {
            var response = new ServiceResponse<FullTeamDTO>();

            if (teamDTO is null)
            {
                response.Message = "Team Data is null";
                response.Success = false;
                return response;
            }

            var teamLead = await _context.Developers
                .FirstOrDefaultAsync(d => d.Id == teamDTO.TeamLeadId);

            if (teamLead is null)
            {
                response.Success = false;
                response.Message = "Team lead not found";
                return response;
            }

            var team = new Team
            {
                Title = teamDTO.Title,
                Description = teamDTO.Description,
                TeamLeadId = teamLead.Id,
                TeamLead = teamLead
            };

            try
            {
                await _context.Teams.AddAsync(team);
                await _context.SaveChangesAsync();

                var TeamDTO = new FullTeamDTO
                {
                    Id = team.Id,
                    Title = team.Title,
                    Description = team.Description,
                    TeamLeadId = team.TeamLeadId,

                    Projects = team.Projects.Select(p => new FullProjectDTO
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        ClientName = p.ClientName,

                    }).ToList(),

                    Developers = team.Developers.Select(d => new FullDeveloperDTO
                    {
                        Id = d.Id,
                        UserName = d.UserName ?? string.Empty,
                        PhoneNumber = d.PhoneNumber ?? string.Empty,
                        Email = d.Email ?? string.Empty,
                    }).ToList(),
                };

                response.Message = "Team created Successfully";
                response.Data = TeamDTO;
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"A database error occured: {dbEx.Message}";
                response.Success = false;
            }

            return response;
        }


        public async Task<ServiceResponse<object>> DeleteDeveloper(int TeamId, string DeveloperId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Developers)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";
                response.Success = false;
                return response;
            }

            if (team.Developers is not null) 
            
            {
                var developer = team.Developers.FirstOrDefault(d => d.Id == DeveloperId);

                if (developer is not null)

                {
                    team.Developers.Remove(developer);

                    try
                    {
                        await _context.SaveChangesAsync();
                        response.Message = "Developer deleted successfully";
                    }

                    catch (DbUpdateException dbEx)
                    {
                        response.Message = $"A database error occured: {dbEx.Message}";
                        response.Success = false;
                    }
                }

                else
                {
                    response.Message = $"This developer is not a member of this Team";
                    response.Success = false;
                }
            }

            else
            {
                response.Message = $"No Developer has been assigned to this Team";
                response.Success = false;
            }

                return response; 
        }

        public async Task<ServiceResponse<object>> DeleteProject(int TeamId, int ProjectId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Projects)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";  
                response.Success = false;
                return response;
            }
            
            if (team.Projects is not null)
            {
                var project = team.Projects.FirstOrDefault(d => d.Id == ProjectId);

                if (project is not null)
                {
                    team.Projects.Remove(project);

                    try
                    {
                        await _context.SaveChangesAsync();
                        response.Message = "Project deleted successfully";
                    }

                    catch (DbUpdateException dbEx)
                    {
                        response.Message = $"A database error occured: {dbEx.Message}";
                        response.Success = false;
                    }
                }

                else
                {
                    response.Message = $"Project is not assigned to this Team";
                    response.Success = false;
                }
            }
            else
            {
                response.Message = $"No project has been assigned to this Team";
                response.Success = false;
            }
                return response;
        }


        public async Task<ServiceResponse<object>> DeleteTeamById(int id)
        {
            var response = new ServiceResponse<object>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if(team is null)
            {
                response.Message = $"Team not found!";
                response.Success = false;
                return response;
            }

            try
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
                response.Message = "Team deleted successfull";
            }

            catch (DbUpdateException dbEx)
            {
                response.Message = $"A database error occured{dbEx.Message}";
                response.Success = false;
            }

            return response;
        }


        public async Task<ServiceResponse<List<FullTeamDTO>>> GetAllTeams(int Page = 1, int PageSize = 10)
        {
            var response = new ServiceResponse<List<FullTeamDTO>>();
            var cacheKey = $"teams:page:{Page}:pageSize:{PageSize}";
            var cachedData = await _cache.GetAsync<List<FullTeamDTO>>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Teams retrieved successfully from cache.";
                return response;
            }

            var teams = await _context.Teams.Include(t => t.Developers)
                                            .Include(t => t.Projects)
                                            .Include(t => t.TeamLead)   
                                            .Skip((Page - 1) * PageSize)
                                            .Take(PageSize)
                                            .ToListAsync();
            if (teams.Count is 0)
            {
                response.Message = "No records found!";
                response.Success = false;
                return response;
            }
 
            var teamsPerPageDTO = new List<FullTeamDTO>();
            
            foreach(var team in teams)
            {
                teamsPerPageDTO.Add(new FullTeamDTO
                {
                    Id = team.Id,
                    Title = team.Title,
                    Description = team.Description,

                    Developers = team.Developers.Count <= 0 ? new List<FullDeveloperDTO>() : team.Developers.Select(d => new FullDeveloperDTO
                    {
                        Id = d.Id,
                        FirstName = d.FirstName,
                        SecondName = d.LastName,
                        UserName = d.UserName ?? string.Empty,
                        PhoneNumber = d.PhoneNumber ?? string.Empty,
                        Email = d.Email ?? string.Empty,
                    }).ToList(),

                    Projects = team.Projects.Count <= 0 ? new List<FullProjectDTO>() : team.Projects.Select(p => new FullProjectDTO
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        ClientName = p.ClientName,

                    }).ToList(),

                    TeamLead = team.TeamLead is null ? null : new FullDeveloperDTO
                    {
                        Id = team.TeamLead.Id,
                        UserName = team.TeamLead.UserName ?? string.Empty,
                        Email = team.TeamLead.Email ?? string.Empty,
                        PhoneNumber = team.TeamLead.PhoneNumber ?? string.Empty,
                    },

                });
            }

            response.Message = "Developers retrieved successfully +" +
                                $"Current Page: {Page}" +
                                $"PageSize: {PageSize}";
                                
            response.Data = teamsPerPageDTO;
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            return response;

        }

 
        public async Task<ServiceResponse<FullTeamDTO>> GetTeamById(int id)
        {
            var response = new ServiceResponse<FullTeamDTO>();

            var cacheKey = $"team:{id}";
            var cachedData = await _cache.GetAsync<FullTeamDTO>(cacheKey);

            if(cachedData is not null)
            {
                response.Data = cachedData;
                response.Message = "Team retrieved from cache";
                return response;
            }
            
            var team = await _context.Teams.Include(t => t.Projects)
                                           .Include(t => t.Developers)
                                           .Include(T => T.TeamLead)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team not found!";
                response.Success = false;
                return response;
            }

            var teamDTO = new FullTeamDTO
            {
                Title = team.Title,  
                Description = team.Description,

                Projects = team.Projects.Count <= 0 ? new List<FullProjectDTO>() : team.Projects.Select(p => new FullProjectDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ClientName = p.ClientName
                }).ToList(),

                Developers = team.Developers.Count <= 0 ? new List<FullDeveloperDTO>() : team.Developers.Select(d => new FullDeveloperDTO  
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    SecondName = d.LastName,
                    UserName = d.UserName ?? string.Empty,
                    PhoneNumber = d.PhoneNumber ?? string.Empty ,
                    Email = d.Email ?? string.Empty,
                }).ToList(),
                   
                TeamLead = team.TeamLead is null ? null : new FullDeveloperDTO
                {
                    Id = team.TeamLead.Id,
                    FirstName = team.TeamLead.FirstName,
                    SecondName = team.TeamLead.LastName,  
                    UserName = team.TeamLead.UserName ?? string.Empty,
                    PhoneNumber = team.TeamLead.PhoneNumber ?? string.Empty,       
                    Email = team.TeamLead.Email ?? string.Empty
                },
    
            };
               
            response.Message = "Team retrieved successfully";
            
            await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<ServiceResponse<object>> PatchTeamById(int id, JsonPatchDocument<TeamDTO> patchData)
        {
            var response = new ServiceResponse<object>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team not found!";
                response.Success = false;
                return response;
            }

            var teamDTO = new TeamDTO
            {
                Title = team.Title,
                Description = team.Description,
                TeamLeadId = team.TeamLeadId,
            };

            patchData.ApplyTo(teamDTO);
            
            team.Title = teamDTO.Title;
            team.Description = teamDTO.Description;
            team.TeamLeadId = teamDTO.TeamLeadId;

            try
            {
               await _context.SaveChangesAsync();
               response.Message = "Team patched successfully";
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"A database error occured: {dbEx.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<object>> UpdateTeamById(int id, TeamDTO teamDTO)
        {
            var response = new ServiceResponse<object>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team not found!";
                response.Success = false;
                return response;
            }

            try
            {
                team.Title = teamDTO.Title;
                team.Description = teamDTO.Description;
                team.TeamLeadId = teamDTO.TeamLeadId;

                response.Message = "Team Updated Successfully";
            }

            catch(DbUpdateException dbEx)
            {
                response.Message = $"Database error occured: {dbEx.Message}";
                response.Success = false;
            }
             
            return response;
        }

        
    }
}
