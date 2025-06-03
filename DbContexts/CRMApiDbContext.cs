using CRMApi.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.DbContexts
{
    public class CRMApiDbContext(DbContextOptions<CRMApiDbContext> options) : DbContext(options)
    {
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Developer>()
                .HasMany(d => d.Teams)  // Developer has many Teams
                .WithMany(t => t.Developers)  // Team has many Developers
                .UsingEntity<Dictionary<string, object>>(
                    "DeveloperTeam",  
                    j => j.HasOne<Team>().WithMany().HasForeignKey("TeamId"),  
                    j => j.HasOne<Developer>().WithMany().HasForeignKey("DeveloperId")  
                );


            // === Team - TeamLead (One-to-Many) === 
            modelBuilder.Entity<Team>()
                .HasOne(t => t.TeamLead)
                .WithMany() // no reverse nav prop
                .HasForeignKey(t => t.TeamLeadId)
                .OnDelete(DeleteBehavior.SetNull); 
        }

    }
}
 