using IssueTracker.Models;

namespace IssueTracker.Utils
{
    public static class ContextUtils
    {
        public static IQueryable<Project> GetUserProjects(this IssueTrackerContext context, int userId)
        {
            if (userId < 0)
                return Enumerable.Empty<Project>().AsQueryable();

            var userProjectIds = context?.PersonProjects?.Where(proj => proj.PersonId == userId).Select(proj => proj.ProjectId);

            if (userProjectIds?.Any() != true)
                return Enumerable.Empty<Project>().AsQueryable();

            var projects = context?.Projects?.Where(proj => userProjectIds.Contains(proj.ProjectId)) ?? Enumerable.Empty<Project>().AsQueryable(); ;

            return projects;
        }
    }
}
