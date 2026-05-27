namespace CRMApi.Domain.DTOs
{
    public class DeveloperLoginResponseDTO
    {
        public LoggedInDeveloperDTO? User { get; set; }
        public string? Token { get; set; }   
    }
}
