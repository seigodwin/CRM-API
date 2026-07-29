
using CRMApi.Domain.DTOs;
using CRMApi.Domain.Models;

namespace CRMApi.Mappings
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

          public static ProjectDTO ToUpdateDto(this Project entity)
        {
            return new ProjectDTO
            {
                Title = entity.Title,
                ClientName = entity.ClientName,
                Description = entity.Description,
                TeamId = entity.TeamId,
                Status = entity.Status
            };
        }


        public static GetProjectDto ToGetDto(this Project entity)
        {
            return new GetProjectDto
            {
                Title = entity.Title,
                ClientName = entity.ClientName,
                Description = entity.Description,
                TeamTitle = entity.Team?.Title ?? "",
                TeamId = entity.TeamId,
                
                Status = entity.Status
            };
        }

        public static GetProjectDto ToPartialGetDto(this Project entity)
        {
            return new GetProjectDto
            {
                Id = entity.Id,
                Title = entity.Title,
            };
        }
    }

}