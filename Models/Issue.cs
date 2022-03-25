using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Issue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Column(TypeName = "VARCHAR (200)")]
        public string? Summary { get; set; }

        [StringLength(2000)]
        [Column(TypeName = "VARCHAR (2000)")]
        public string? Description { get; set; }

        public IssueStatus State { get; set; }

        [Required]
        public IssuePriority Priority { get; set; }

        [DataType(DataType.Date)]
        public DateTime TargetResolutionDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ActualResolutionDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int AsigneeId { get; set; }
        public Person? Asignee { get; set; }

        [ForeignKey("Person")]
        public int CreatorId { get; set; }
        public Person? Creator { get; set; }

    }

    public enum IssuePriority
    {
        Low,
        Medium,
        High
    }

    public enum IssueStatus
    {
        New,
        Active,
        Resolved,
        Closed
    }
}
