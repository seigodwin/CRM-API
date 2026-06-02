using CRMApi.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class FullDeveloperDTO
    {
        public string Id { get; set; } = string.Empty;
        public  string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public required string UserName { get; set; }   
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;
        [EmailAddress]
        public required string Email { get; set; } = string.Empty;
        public List<string> Stack { get; set; } = new List<string>();
        public List<string> Roles {get; set;} = new List<string>();
        public List<FullTeamDTO>? Teams { get; set; } = new List<FullTeamDTO>();

    }
}
