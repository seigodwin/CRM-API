
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;

namespace CRMApi.Extentions
{
    public static class ToEntityMapping
    {
        public static Team ToEntity(this TeamDTO dto)
        {
            return new Team
            {
                Title = dto.Title,
                Description = dto.Description,
                TeamLeadId = dto.TeamLeadId
            };
        }
    }

    public static class ToTeamDtoMappings
    {
        public static FullTeamDTO ToTeamDto(this Team entity)
        {
            return new FullTeamDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                TeamLeadId = entity.TeamLeadId
            };
        }
    }
}