
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class AuthenticatedUsertDto
    {
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        // 👇 Add this property here
        public string RefreshToken { get; set; } = string.Empty; 
        public DateTime AccessTokenExpiration { get; set; }
    }
}
