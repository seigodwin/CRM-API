using CRMApi.Domain.Models;

namespace CRMApi.Domain.DTOs
{
    public class FullTeamDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FullProjectDTO> Projects { get; set; } = new List<FullProjectDTO>();
        public List<FullDeveloperDTO> Developers { get; set; } = new List<FullDeveloperDTO>();
        public FullDeveloperDTO? TeamLead { get; set; }
        public string TeamLeadId { get; set; } = string.Empty;
    }
}
