using CRM_API.Domain.DTOs.AuthDtos;
using CRMApi.Domain.Models;

namespace CRMApi.Mappings
{
    public static class ToEntityMappings                                                   
    {                                                           
        public static Developer ToEntity(this RegisterDeveloperRequestDto dto)                                                              
        {
            return new Developer                                                                  
            {                                                                        
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber
            };          
        }
    }

    public static class ToDtoMappings
    {
        public static RegisterDeveloperResponseDto ToDto(this Developer entity)
        {
            return new RegisterDeveloperResponseDto          
            {           
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                UserName = entity.UserName ?? entity.Email!,
                Email = entity.Email!,
                PhoneNumber = entity.PhoneNumber
            };
        }
    }
}