using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class IssueDetails
    {
        public Issue? issue { get; set; }

        public List<PersonProject>? personProjects;

        public Person? currentUser;

        public List<Comment>? comments;

        public ProjectRole userRole;
    }
}
