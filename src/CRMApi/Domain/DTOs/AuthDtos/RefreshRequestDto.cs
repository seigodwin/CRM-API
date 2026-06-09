
namespace CRM_API.Domain.DTos
{
    public class RefreshRequestDto
    {
        public required string AccessToken { get; set; } 
        public required string RefreshToken { get; set; } 
    }
}