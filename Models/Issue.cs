using System.ComponentModel.DataAnnotations;


namespace IssueTracker.Models
{
    public class Issue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string? Summary { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [Required]
        public int Priority { get; set; }

        [DataType(DataType.Date)]
        public DateTime TargetResolutionDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ActualResolutionDate { get; set; }

        public int AssignedTo { get; set; }

        public int ProjectId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        public string? CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedOn { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
