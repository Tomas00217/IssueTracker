using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR (50)")]
        public string? FirstName { get; set; }

        [StringLength(50)]
        [Column(TypeName = "VARCHAR (50)")]
        public string? SecondName { get; set; }

        [Required]
        [EmailAddress]
        [Column(TypeName = "VARCHAR (255)")]
        public string? Email { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR (64)")]
        public string? Password { get; set; }

    }
}
