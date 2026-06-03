
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class RegisterDeveloperResponseDto
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        [MaxLength(50)]
        public string FirstName {get;set;} = string.Empty;
        public string LastName {get;set;} = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Roles {get; set;} = new List<string>();
        public List<string> Stack {get; set;} = new List<string>();
        
    }
}