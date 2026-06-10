
using System.ComponentModel.DataAnnotations;

namespace CRM_API.Domain.DTOs.AuthDtos
{
    public class ChangePasswordRequestDto
    {
        [EmailAddress]
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string  CurrentPassword { get; set; } 
        [DataType(DataType.Password)]
        public required string NewPassword { get; set; } 
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public required string ConFirmNewPassword {get ; set;}
        

    }
}