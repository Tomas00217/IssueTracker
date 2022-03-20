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
        public int Role { get; set; }

    }
}
