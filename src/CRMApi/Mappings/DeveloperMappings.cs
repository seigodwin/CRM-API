using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Domain.DTOs;
using CRMApi.Domain.DTOs.DeveloperDTOs;
using CRMApi.Domain.Models;

namespace CRMApi.Mappings
{
    public static class ToEntityMappings                                                   
    {                                                           
        public static Developer ToEntity(this CreateDeveloperDto dto)                                                              
        {
            return new Developer                                                                  
            {                                                                        
                FirstName = dto.FirstName,
                LastName = dto.SecondName,
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Stack = dto.Stack
            };          
        }

           public static GetDeveloperDTO ToGetDto(this Developer entity)
        {
            return new GetDeveloperDTO          
            {           
                Id = entity.Id,
                FullName = $"{entity.FirstName} {entity.LastName}",
                UserName = entity.UserName ?? entity.Email!,
                Email = entity.Email!,
                PhoneNumber = entity.PhoneNumber ?? "",
                Teams = entity.Teams.Select( t => t.ToPartiallGetDto()).ToList()
            };
        }

        
        public static void Update(this Developer entity , CreateDeveloperDto dto)
        {
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.SecondName;
            entity.UserName = dto.UserName;
            entity.Email = dto.Email;
            entity.PhoneNumber = dto.PhoneNumber;
            entity.Stack = dto.Stack;
        }

             public static CreateDeveloperDto ToPatchDto(this Developer entity)
        {
            return new CreateDeveloperDto          
            {           
                FirstName = entity.FirstName,
                SecondName = entity.LastName,
                UserName = entity.UserName ?? entity.Email!,
                Email = entity.Email!,
                PhoneNumber = entity.PhoneNumber ?? "",
                Teams = entity.Teams.Select( t => t.ToPartiallGetDto()).ToList()
            };
        }

        public static GetDeveloperDTO ToPartiallGetDto(this Developer entity)
        {
            return new GetDeveloperDTO          
            {           
                Id = entity.Id,
                FullName = $"{entity.FirstName} {entity.LastName}"
            };
        }
    }

}