using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs.ProjectDTOs
{
    public class UpdateProjectRequestDTO
    {
        public IFormFile? Image { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ClientName { get; set; }
        public DateTime? DateStarted { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;
        public int? TeamId { get; set; }   
    }
}
