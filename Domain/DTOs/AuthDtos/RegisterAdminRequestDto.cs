
namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class RegisterAdminRequestDto
    {
        public required string FirstName {get; set;}
        public required string LastName {get; set;}
        public string Username { get; set; } = string.Empty;
        public required string Email { get; set; } 
        public required string Password { get; set; } 
        public List<string> Roles {get; set;} = new List<string>();
}

}