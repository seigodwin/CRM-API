namespace CRMApi.Domain.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<Project>? Projects { get; set; }
        public List<Developer>? Developers { get; set; }   
        public Developer? TeamLead { get; set; }  
        public int? TeamLeadId { get; set; } 

    }
}
