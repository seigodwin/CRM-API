using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class LoggedInUserDTO
    {

        public string? Id { get; set; }
        public IFormFile? Image { get; set; }
        public  string? FirstName { get; set; }
        public  string? LastName { get; set; }
        public string? UserName { get; set; }
        [EmailAddress]
        public  string? Email { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
    }


}
