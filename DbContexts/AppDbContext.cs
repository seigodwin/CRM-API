using CRMApi.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMApi.DbContexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Team> Teams { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();



            // === Team - TeamLead (One-to-Many) === 
            modelBuilder.Entity<Team>()
                .HasOne(t => t.TeamLead)
                .WithMany() // no reverse nav prop
                .HasForeignKey(t => t.TeamLeadId)
                .OnDelete(DeleteBehavior.SetNull); 
             
        }

    }
}
 