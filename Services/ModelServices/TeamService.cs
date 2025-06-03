using System.Linq;
using Azure.Storage.Blobs;
using CRMApi.DbContexts;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;
using CRMApi.Services.Interfaces;
using CRMApi.Utility.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.Services.ModelServices
{
    public class TeamService(CRMApiDbContext context, BlobServiceClient _blobServiceClient) : ITeamService
    {
        private readonly BlobServiceClient _blobServiceClient = _blobServiceClient;
        private readonly string _blobContainerName = "photos";
        private readonly CRMApiDbContext _context = context;

        public async Task<ServiceResponse<bool>> AssignDeveloperToTeam(int DeveloperId, int TeamId)
        {
            var response = new ServiceResponse<bool>();

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
                    response.Message = $"Developer with Id {DeveloperId} exists in Team with Id {TeamId} already";
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
         
        

        public async Task<ServiceResponse<bool>> AssignProjectToTeam(int ProjectId, int TeamId)
        {
            var response = new ServiceResponse<bool>();

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

            if (team.Projects.Count is not 0)
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
                team.Projects.Add(project);
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

            var team = new Team
            {
                Title = teamDTO.Title,
                Description = teamDTO.Description,
                TeamLeadId = teamDTO.TeamLeadId,
                TeamLead = await _context.Developers.FirstOrDefaultAsync( d => d.Id == teamDTO.TeamLeadId)
            };

            try
            {
                if (teamDTO.Image != null && teamDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(teamDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = teamDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob   
                    team.ImageUrl = blobClient.Uri.ToString();
                }

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
                        Name = d.Name,
                        ImageUrl = d.ImageUrl,
                        Contact = d.Contact,
                        Email = d.Email,
                    }).ToList(),

                    ImageUrl = team.ImageUrl
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


        public async Task<ServiceResponse<bool>> DeleteDeveloper(int TeamId, int DeveloperId)
        {
            var response = new ServiceResponse<bool>();

            var team = await _context.Teams.Include(t => t.Developers)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";
                response.Success = false;
                return response;
            }


            if (team.Developers.Any(d => d.Id == DeveloperId))
            {
                var developer = team.Developers.FirstOrDefault(d => d.Id == DeveloperId);

                team.Developers.Remove(developer);
                try
                {
                    await _context.SaveChangesAsync();
                    response.Message = "Developer deleted successfully";
                }

                catch(DbUpdateException dbEx)
                {
                    response.Message = $"A database error occured: {dbEx.Message}";
                    response.Success= false;
                }
            }

            else
            {
                response.Message = $"Developer with Id {DeveloperId} is not a member of this Team";
                response.Success = false;
            }

            return response; 
        }



        public async Task<ServiceResponse<bool>> DeleteProject(int TeamId, int ProjectId)
        {
            var response = new ServiceResponse<bool>();

            var team = await _context.Teams.Include(t => t.Projects)
                                           .FirstOrDefaultAsync(t => t.Id == TeamId);

            if (team is null)
            {
                response.Message = $"Team with Id {TeamId} not found!";  
                response.Success = false;
                return response;
            }


            if (team.Projects.Any(d => d.Id == ProjectId))
            {
                var project = team.Projects.FirstOrDefault(d => d.Id == ProjectId);

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
                response.Message = $"Project with Id {ProjectId} is not assigned to this Team";
                response.Success = false;
            } 

            return response;
        }


        public async Task<ServiceResponse<bool>> DeleteTeamById(int id)
        {
            var response = new ServiceResponse<bool>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if(team is null)
            {
                response.Message = $"Team with Id: {id} not found!";
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

            var totalTeams = _context.Teams.Count();
            var totalPages = (int)Math.Ceiling((decimal)totalTeams / PageSize);


            var teamsPerPageDTO = new List<FullTeamDTO>();
            
            foreach(var team in teams)
            {
                 teamsPerPageDTO.Add(new FullTeamDTO
                {
                    Id = team.Id,
                    Title = team.Title,
                    Description = team.Description,
                    ImageUrl = team.ImageUrl,

                    Developers = team.Developers.Select(d => new FullDeveloperDTO
                    {
                        Id = d.Id,
                        Name = d.Name,
                        ImageUrl = d.ImageUrl,
                        Contact = d.Contact,
                        Email = d.Email,
                    }).ToList(),
                    
                    Projects = team.Projects.Select(p => new FullProjectDTO 
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description= p.Description,
                        ClientName = p.ClientName,

                    }).ToList(),

                    TeamLead = new FullDeveloperDTO
                    {
                        Id = team.TeamLead.Id,
                        Name = team.TeamLead.Name,
                        Email = team.TeamLead.Email,
                        Contact = team.TeamLead.Contact,
                        ImageUrl= team.TeamLead.ImageUrl,
                    },
                    
                }); 
            }

            response.Message = "Developers retrieved successfully +" +
                                $"Current Page: {Page}" +
                                $"PageSize: {PageSize}" +
                                $"Total Pages: {totalPages}";
            response.Data = teamsPerPageDTO;
            return response;

        }

 
        public async Task<ServiceResponse<FullTeamDTO>> GetTeamById(int id)
        {
            var response = new ServiceResponse<FullTeamDTO>();
            var team = await _context.Teams.Include(t => t.Projects)
                                           .Include(t => t.Developers)
                                           .Include(T => T.TeamLead)
                                           .FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team with Id: {id} not found!";
                response.Success = false;
                return response;
            }

            var teamDTO = new FullTeamDTO
            {
                Title = team.Title,  
                ImageUrl = team.ImageUrl,
                Description = team.Description,

                Projects = team.Projects.Select(p => new FullProjectDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ClientName = p.ClientName
                }).ToList(),

                Developers = team.Developers.Select(d => new FullDeveloperDTO  
                {
                    Id = d.Id,
                    Name = d.Name,
                    Contact = d.Contact,
                    Email = d.Email
                }).ToList(),
                   
                TeamLead = new FullDeveloperDTO
                {
                    Id = team.TeamLead.Id,
                    Name = team.TeamLead.Name,
                    Contact = team.TeamLead.Contact,  
                    Email = team.TeamLead.Email
                },
    
            };
               
            response.Message = "Team retrieved successfully";
            return response;
        }


        public async Task<ServiceResponse<bool>> PatchTeamById(int id, JsonPatchDocument<TeamDTO> patchData)
        {
            var response = new ServiceResponse<bool>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team with Id: {id} not found!";
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


        public async Task<ServiceResponse<bool>> UpdateTeamById(int id, TeamDTO teamDTO)
        {
            var response = new ServiceResponse<bool>();
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

            if (team is null)
            {
                response.Message = $"Team with Id: {id} not found!";
                response.Success = false;
                return response;
            }

            try
            {
                team.Title = teamDTO.Title;
                team.Description = teamDTO.Description;
                team.TeamLeadId = teamDTO.TeamLeadId;

                if (teamDTO.Image != null && teamDTO.Image.Length > 0)
                {
                    // Get a reference to the blob container
                    var containerClient = _blobServiceClient.GetBlobContainerClient(_blobContainerName);

                    // Generate a unique file name for the blob
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(teamDTO.Image.FileName)}";

                    // Get a reference to the blob
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload the file to Blob Storage
                    using (var stream = teamDTO.Image.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, true); // overwrite if it exists
                    }

                    // Get the public URL of the uploaded blob   
                    team.ImageUrl = blobClient.Uri.ToString();
                }

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
