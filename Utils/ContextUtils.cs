using IssueTracker.Models;

namespace IssueTracker.Utils
{
    public static class ContextUtils
    {
        public static List<Project> GetUserProjects(this IssueTrackerContext context, int userId)
        {
            if (userId < 0)
                return new List<Project>();

            var userProjectIds = context?.PersonProjects?.Where(proj => proj.PersonId == userId).Select(proj => proj.ProjectId);

            if (userProjectIds?.Any() != true)
                return new List<Project>();

            var projects = context?.Projects?.Where(proj => userProjectIds.Contains(proj.ProjectId)).ToList() ?? new List<Project>();

            return projects;
        }
    }
}
