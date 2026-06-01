
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class AssignRoleRequestDto
    {
        [EmailAddress]
        public string Email {get; set;} = string.Empty;
        List<string> Roles = new List<string>();
    }
}