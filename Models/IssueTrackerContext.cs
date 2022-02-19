using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Models
{
    public class IssueTrackerContext : DbContext
    {
        public DbSet<Project> ?Projects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(@"Data Source=D:\Projekty\IssueTracker\app.db");

    }
}
