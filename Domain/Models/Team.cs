namespace CRMApi.Domain.Models
{
    public class Team
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public List<Project> Projects { get; set; } = new ();
        public List<Developer> Developers { get; set; } = new ();  
        public Developer? TeamLead { get; set; }  
        public string? TeamLeadId { get; set; } 

    }
}
