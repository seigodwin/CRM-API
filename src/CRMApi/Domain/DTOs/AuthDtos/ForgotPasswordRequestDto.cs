
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTos.AuthDtos
{
    public class ForgotPasswordRequestDto
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}