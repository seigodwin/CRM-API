namespace CRMApi.Domain.Models
{
    public class User
    {
      public int Id { get; set; }
      public required string FirstName { get; set; }
      public required string LastName { get; set; }
      public string? ImageUrl { get; set; }
      public required string Email { get; set; }
      public required string Password { get; set; }
      public string? OTP { get; set; }
      public DateTime? OTPExpiration { get; set; }
    }
}
