using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs.DeveloperDTOs
{
    public class DevRegistrationRequestDTO
    {
        public IFormFile? Image { get; set; }
        public required string FirstName { get; set; }
        public required string SecondName { get; set; }
        public  string? UserName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }  
        [Phone]
        public string? PhoneNumber { get; set; }
        [PasswordPropertyText]
        public required string Password { get; set; }
        public List<String>? Stack { get; set; }
    }
}
