using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class LoginRequestDTO
    {
        [EmailAddress]
        public required string Email { get; set; }
        [PasswordPropertyText]
        public required string Password { get; set; }
    }
}
