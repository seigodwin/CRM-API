
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class LoginRequestDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
