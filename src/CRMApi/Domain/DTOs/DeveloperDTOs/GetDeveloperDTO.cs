using CRMApi.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class GetDeveloperDTO
    {
        public string Id { get; set; } = string.Empty;  
        public string FullName {get ; set;} = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public  string Email { get; set; } = string.Empty;
        public List<string> Stack { get; set; } = new List<string>();
        public List<string> Roles {get; set;} = new List<string>();
        public List<GetTeamDto> Teams { get; set; } = new List<GetTeamDto>();

    }
}
