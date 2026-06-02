using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class TeamDTO
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public string? TeamLeadId { get; set; }

    }
}
