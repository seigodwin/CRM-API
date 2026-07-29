using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class GetProjectDto
    {
        public int Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime? DateStarted { get; set; }
        public DateTime? DateCompleted { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;
        public string TeamTitle { get; set; } = string.Empty;
        public int? TeamId { get; set; }
    }
}
