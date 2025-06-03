using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class FullDeveloperDTO
    {
        public int? Id { get; set; }
        public string? ImageUrl { get; set; }
        public required string Name { get; set; }
        public required string Contact { get; set; }
        public required string Email { get; set; }
        public List<string>? Stack { get; set; }
        public List<FullTeamDTO>? Teams { get; set; }

    }
}
