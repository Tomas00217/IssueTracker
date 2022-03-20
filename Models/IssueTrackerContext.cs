using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Models
{
    public class IssueTrackerContext : DbContext
    {
        public DbSet<Project>? Projects { get; set; }
        public DbSet<Issue>? Issues { get; set; }
        public DbSet<Person>? Persons { get; set; }
        public DbSet<PersonProject>? PersonProjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=D:\Projekty\IssueTracker\app.db");
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<PersonProject>()
                .HasKey(c => new { c.PersonId, c.ProjectId });
        }

    }
}
