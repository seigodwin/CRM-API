
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Exceptions.Types;
using CRMApi.Mappings;
using CRMApi.Repository;
using CRMApi.Services.Interfaces;
using CRMApi.Utility;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.ModelServices
{
    public class TeamService(AppDbContext context, 
    IBaseRepository<Team> repo, IDistributedRedisCacheService cache,
    ILogger<TeamService> logger) : ITeamService
    {
        private readonly AppDbContext _context = context;
        private readonly IBaseRepository<Team> _repo = repo;
        private readonly IDistributedRedisCacheService _cache = cache;
        private readonly ILogger<TeamService> _logger = logger;

        public async Task<ServiceResponse<object>> AssignDeveloperToTeam(string DeveloperId, int TeamId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Developers)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                throw new NotFoundException($"Team not found!");
            }

            var developer = await _context.Developers.FirstOrDefaultAsync(d => d.Id == DeveloperId);

            if (developer is null)
            {
                throw new NotFoundException($"Developer not found!");
            }

            if (team.Developers.Count is not 0)
            {
                if (team.Developers.Any(d => d.Id == DeveloperId))
                {
                    throw new ConflictException($"Developer exists in Team  already");
                }
            }

             team.Developers.Add(developer);
             await  _context.SaveChangesAsync();
          
            response.Message = "Developer added to the Team successfully";

            return response;
        }
         

        public async Task<ServiceResponse<object>> AssignProjectToTeam(int ProjectId, int TeamId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Projects)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
               throw new NotFoundException($"Team not found!"); 
            }

            var project = await _context.Projects.FirstOrDefaultAsync(d => d.Id == ProjectId);

            if (project is null)
            {
                throw new NotFoundException($"Project not found!");
            }

            if (team.Projects is not null && team.Projects.Count is not 0)
            {
                if (team.Projects.Any(p => p.Id == ProjectId))
                {
                    throw new ConflictException($"Project exists in Team already");
                }
            }

            team.Projects?.Add(project);
            await _context.SaveChangesAsync();
            response.Message = "Project assigned to the Team successfully";
            
            return response;
        }

        public async Task<ServiceResponse<GetTeamDto>> CreateTeam(CreateTeamDTO teamDTO)
        {
            var response = new ServiceResponse<GetTeamDto>();

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
               throw new NotFoundException($"Team Lead not found!");
            }

            var team = teamDTO.ToEntity();

                await _context.Teams.AddAsync(team);
                await _context.SaveChangesAsync();

                response.Message = "Team created Successfully";
                response.Data = team.ToGetDto();

            return response;
        }


        public async Task<ServiceResponse<object>> DeleteDeveloper(int TeamId, string DeveloperId)
        {
            var response = new ServiceResponse<object>();

            var team = await _context.Teams.Include(t => t.Developers)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                throw new NotFoundException($"Team not found!");
            }

            if (team.Developers is not null) 
            
            {
                var developer = team.Developers.FirstOrDefault(d => d.Id == DeveloperId);

                if (developer is not null)

                {
                    team.Developers.Remove(developer);


                        await _context.SaveChangesAsync();
                        response.Message = "Developer deleted successfully";
                }

                else
                {
                    throw new NotFoundException($"Developer is not found in Team");
                }
            }

            else
            {
                throw new NotFoundException($"No Developer has been assigned to this Team");
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
                throw new NotFoundException($"Team not found!");
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
                    throw new NotFoundException($"Project is not assigned to this Team");
                }
            }
            else
            {
                throw new NotFoundException($"No project has been assigned to this Team");
            }
            
            return response;
        }


        public async Task<ServiceResponse<object>> DeleteTeamById(int id)
        {
            var response = new ServiceResponse<object>();
            var team = await _repo.GetById(id);

            if(team is null)
            {
                throw new NotFoundException($"Team not found!");
            }

                await _repo.DeleteAsync(team);
                response.Message = "Team deleted successfull";

            return response;
        }


        public async Task<ServiceResponse<List<GetTeamDto>>> GetAllTeams(int Page = 1, int PageSize = 10)
        {
            Page = Page < 1 ? 1 : Page;
            PageSize = PageSize < 1 ? 1 : (PageSize > 30 ? 30 : PageSize);

            var response = new ServiceResponse<List<GetTeamDto>>();
            var cacheKey = $"teams:page:{Page}:pageSize:{PageSize}";
            var cachedData = await _cache.GetAsync<List<GetTeamDto>>(cacheKey);

            if(cachedData != null)
            {
                response.Data = cachedData;
                response.Message = "Teams retrieved successfully from cache.";
                return response;
            }

            var teams = await _context.Teams.AsNoTracking().Include(t => t.Developers)
                                            .OrderBy(t => t.Id)
                                            .Include(t => t.Projects)
                                            .Include(t => t.TeamLead)   
                                            .Skip((Page - 1) * PageSize)
                                            .Take(PageSize)
                                            .ToListAsync();
            if (teams.Count is 0)
            {
                throw new NotFoundException("No teams found.");
            }

            response.Message = "Developers retrieved successfully +" +
                                $"Current Page: {Page}" +
                                $"PageSize: {PageSize}";
                                
            response.Data = teams.Select( t => t.ToGetDto()).ToList();

            try
            {
                await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache teams data. {CacheKey}", cacheKey);
            }

            return response;

        }

 
        public async Task<ServiceResponse<GetTeamDto>> GetTeamById(int id)
        {
            var response = new ServiceResponse<GetTeamDto>();

            var cacheKey = $"team:{id}";
            var cachedData = await _cache.GetAsync<GetTeamDto>(cacheKey);

            if(cachedData is not null)
            {
                response.Data = cachedData;
                response.Message = "Team retrieved from cache";
                return response;
            }
            
            var team = await _context.Teams.AsNoTracking().Include(t => t.Projects)
                                            .AsNoTracking()
                                           .Include(t => t.Developers)
                                           .Include(T => T.TeamLead)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                throw new NotFoundException("Team not found!");
            }

            response.Data = team.ToGetDto();
               
            response.Message = "Team retrieved successfully";

            try
            {
                await _cache.SetAsync(cacheKey, response.Data, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache team data. {CacheKey}", cacheKey);
            }

            return response;
        }

        public async Task<ServiceResponse<object>> PatchTeamById(int id, JsonPatchDocument<CreateTeamDTO> patchData)
        {
            var response = new ServiceResponse<object>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team not found!";
                response.Success = false;
                return response;
            }

            var teamDTO = team.ToPatchDto();

            patchData.ApplyTo(teamDTO);
        
                _context.Teams.Update(team);
               await _context.SaveChangesAsync();
               response.Message = "Team patched successfully";
            
            return response;
        }

        public async Task<ServiceResponse<object>> UpdateTeamById(int id, CreateTeamDTO teamDTO)
        {
            var response = new ServiceResponse<object>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
               throw new NotFoundException($"Team not found!");
            }

                team.Update(teamDTO);
                await _context.SaveChangesAsync();

              response.Message = "Team Updated Successfully";
            
             
            return response;
        }

    }
}
