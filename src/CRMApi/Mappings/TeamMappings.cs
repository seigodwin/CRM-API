
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;

namespace CRMApi.Mappings                           
{           
    public static class ToEntityMapping                                                    
    {                                                           
        public static Team ToEntity(this CreateTeamDTO dto)                                                              
        {
            return new Team                                                                    
            {                                                                        
                Title = dto.Title,                  
                Description = dto.Description,                  
                TeamLeadId = dto.TeamLeadId,
                         
            };          
        }

        public static GetTeamDto ToGetDto(this Team entity)
        {
            var dto = new GetTeamDto        
            {           
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,  
                TeamLeadId = entity.TeamLeadId,
                Developers = entity.Developers.Select(d => d.ToPartiallGetDto()).ToList()
            };

            if(entity.TeamLead is not null)
            {
                dto.TeamLeadName = $"{entity.TeamLead.FirstName} {entity.TeamLead.LastName}";
            }

            return dto;
        }

        public static GetTeamDto ToPartiallGetDto(this Team entity)
        {
             return new GetTeamDto        
            {           
                Id = entity.Id,
                Title = entity.Title,
            };
        }
    }

}   

