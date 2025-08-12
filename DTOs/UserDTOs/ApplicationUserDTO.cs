using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class ApplicationUserDTO
    {
      public string? Id { get; set; }   
      public IFormFile? Image { get; set; }
      public required string FirstName { get; set; }
      public required string LastName { get; set; }
      [EmailAddress]
      public required string Email { get; set; }
      [Phone]
      public string? PhoneNumber { get; set; }
        
      public required string Password { get; set; }
    }
}
