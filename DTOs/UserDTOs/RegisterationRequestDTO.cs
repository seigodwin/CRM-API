using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class RegistrationRequestDTO
    {
        public IFormFile? Image { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        [PasswordPropertyText]
        public required string Password { get; set; }
    }
}
