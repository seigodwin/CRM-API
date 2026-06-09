using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class ResetPasswordDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Token { get; set; } 
        public required string NewPassword { get; set; }
    }
}
