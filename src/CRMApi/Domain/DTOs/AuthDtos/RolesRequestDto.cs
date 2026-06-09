using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs.AuthDtos
{
    public class RolesRequestDto
    {
        [MaxLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public required string RoleName { get; set; }
    }
}