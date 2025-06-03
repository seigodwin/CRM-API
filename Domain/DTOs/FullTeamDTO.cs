using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class FullTeamDTO
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<FullProjectDTO>? Projects { get; set; }
        public List<FullDeveloperDTO>? Developers { get; set; }
        public FullDeveloperDTO? TeamLead { get; set; }
        public int? TeamLeadId { get; set; }
    }
}
