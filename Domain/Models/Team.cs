namespace CRMApi.Domain.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<Project>? Projects { get; set; }
        public List<ApplicationUser>? Developers { get; set; }   
        public ApplicationUser? TeamLead { get; set; }  
        public string? TeamLeadId { get; set; } 

    }
}
