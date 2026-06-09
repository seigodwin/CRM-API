
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class AssignRoleRequestDto
    {
        [EmailAddress]
        public required string Email {get; set;} 
        public List<string> Roles{get; set;} = new List<string>();
    }
}