namespace CRMApi.Domain.DTOs
{
    public class FullUserDTO
    {
    
            public int Id { get; set; }
            public IFormFile Image { get; set; }
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required string Email { get; set; }
            public required string Password { get; set; }
        
    }


}
