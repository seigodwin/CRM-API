using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs.DeveloperDTOs
{
    public class CreateDeveloperDto
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string SecondName { get; set; } = string.Empty;
        public required string UserName { get; set; } = string.Empty;
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }
        [EmailAddress]
        public required string Email { get; set; } 
        public List<string> Stack { get; set; } = new List<string>();
        public List<GetTeamDto> Teams { get; set; } = new List<GetTeamDto>();
    }
}
