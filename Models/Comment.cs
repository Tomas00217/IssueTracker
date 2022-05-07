using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("Issue")]
        public int IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        [ForeignKey("Person")]
        public int PersonId { get; set; }
        public Person? Person { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EditedOn { get; set; }

        [StringLength(2000)]
        [Column(TypeName = "VARCHAR (2000)")]
        public string? Description { get; set; }

    }
}
