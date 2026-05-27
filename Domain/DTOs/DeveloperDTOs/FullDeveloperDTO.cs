using CRMApi.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace CRMApi.Domain.DTOs
{
    public class FullDeveloperDTO
    {
        public string? Id { get; set; }
        public string? ImageUrl { get; set; }
        public  string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public required string UserName { get; set; }   
        [Phone]
        public string? PhoneNumber { get; set; }
        [EmailAddress]
        public required string Email { get; set; }
        public List<string>? Stack { get; set; }
        public List<FullTeamDTO>? Teams { get; set; }

    }
}
