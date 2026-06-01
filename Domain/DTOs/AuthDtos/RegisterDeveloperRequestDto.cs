
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class RegisterDeveloperRequestDto
    {
        [MaxLength(50)]
         public required string FirstName { get; set; }
         [MaxLength(50)]
        public required string LastName { get; set; }
        [MaxLength(50)]
        public  string? UserName { get; set; }
        [EmailAddress]
        public required string Email { get; set; }  
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public List<String>? Stack { get; set; }
        public List<string>? Roles {get; set;}
    }
}