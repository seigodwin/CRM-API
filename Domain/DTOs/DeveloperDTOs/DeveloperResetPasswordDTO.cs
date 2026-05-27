using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class DeveloperResetPasswordDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Token { get; set; } 
        public required string NewPassword { get; set; }
    }
}
