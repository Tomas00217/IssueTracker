using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class ProjectDetails
    {
        public Project Project { get; set; }
        
        public IQueryable<int> projectIds;

        public string projectLead;

        public PaginatedList<Issue> issues;

        public ProjectRole userRole;

        public IQueryable<PersonProject> personProjects;

        public List<Person> allUsers;
    }
}
