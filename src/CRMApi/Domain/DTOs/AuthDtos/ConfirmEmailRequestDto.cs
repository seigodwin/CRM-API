
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class ConfirmEmailRequestDto
    {
        [EmailAddress]
        public string Email {get;set;} = string.Empty;
        public string Token {get;set;} = string.Empty;
    }
}