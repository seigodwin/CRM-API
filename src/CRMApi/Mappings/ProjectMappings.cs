
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;

namespace CRMApi.Extentions
{
    public static class ToEntityMaaping
    {
        public static Project ToEntity(this ProjectDTO dto)
        {
            return new Project
            {
                Title = dto.Title,
                ClientName = dto.ClientName,
                Description = dto.Description,
                TeamId = dto.TeamId
            };
        }
    }
}