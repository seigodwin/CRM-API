using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class FullProjectDTO
    {
        public int Id { get; set; } 
        public string? ImageUrl { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ClientName { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;
        public FullTeamDTO? Team { get; set; }
        public int? TeamId { get; set; }
    }
}
