using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Person_Project
    {
        [ForeignKey("PersonForeignKey")]
        public Person? Person { get; set; }
        [ForeignKey("ProjectForeignKey")]
        public Project? Project { get; set; }
        [StringLength(100)]
        public string? Role { get; set; }

    }
}
