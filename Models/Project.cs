using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Project
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "VARCHAR (100)")]
        public string? Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TargetEndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ActualEndDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

    }

}