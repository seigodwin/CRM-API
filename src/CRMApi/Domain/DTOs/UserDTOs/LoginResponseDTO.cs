namespace CRMApi.Domain.DTOs
{
    public class LoginResponseDTO
    {
        public LoggedInUserDTO? User { get; set; }
        public string? Token { get; set; }   
    }
}
