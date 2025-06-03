namespace CRMApi.Domain.Models
{
    public class Developer
    {
        public int Id { get; set; } 
        public string? ImageUrl { get; set; }
        public required string Name { get; set; } 
        public required string Contact { get; set; } 
        public required string Email { get; set; }  
        public List<string>? Stack { get; set; } 
        public List<Team>? Teams { get; set; }   
    }
}
