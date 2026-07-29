

namespace CRMApi.Domain.DTOs
{
    public class GetTeamDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<GetProjectDto> Projects { get; set; } = new List<GetProjectDto>();
        public List<GetDeveloperDTO> Developers { get; set; } = new List<GetDeveloperDTO>();
        public string TeamLeadName { get; set; } = string.Empty;
        public string TeamLeadId { get; set; } = string.Empty;
    }
}
