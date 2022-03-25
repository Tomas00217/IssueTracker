using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class PersonProject
    {

        [ForeignKey("Person")]
        public int PersonId { get; set; }
        public Person? Person { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        public Project? Project { get; set; }

        [StringLength(100)]
        public ProjectRole Role { get; set; }

    }

    public enum ProjectRole
    {
        Manager,
        ProjectLead,
        Developer
    }
}
