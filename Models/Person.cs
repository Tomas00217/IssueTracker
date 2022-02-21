using System.ComponentModel.DataAnnotations;


namespace IssueTracker.Models
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string? SecondName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string Password { get; set; }

    }
}
