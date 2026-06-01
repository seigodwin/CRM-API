
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class AuthenticatedUsertDto
    {
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
