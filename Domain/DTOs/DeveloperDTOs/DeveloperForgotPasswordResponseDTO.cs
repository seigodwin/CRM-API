using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class DeveloperForgotPasswordResponseDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        public required string Token { get; set; }  
    }
}
