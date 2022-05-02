using IssueTracker.Models;
namespace IssueTracker.Utils.Projects
{
    public static class ProjectSorts
    {
        public static IQueryable<Project> sortByOrder(IQueryable<Project> projects, string sortOrder)
        {
            switch (sortOrder)
            {
                case "NameDesc":
                    projects = projects.OrderByDescending(p => p.Name);
                    break;
                case "StartDate":
                    projects = projects.OrderBy(p => p.StartDate);
                    break;
                case "StartDateDesc":
                    projects = projects.OrderByDescending(p => p.StartDate);
                    break;
                case "TargetDate":
                    projects = projects.OrderBy(p => p.TargetEndDate);
                    break;
                case "TargetDateDesc":
                    projects = projects.OrderByDescending(p => p.TargetEndDate);
                    break;
                case "Status":
                    projects = projects.OrderBy(p => p.TargetEndDate);
                    break;
                case "StatusDesc":
                    projects = projects.OrderByDescending(p => p.TargetEndDate);
                    break;
                default:
                    projects = projects.OrderBy(p => p.Name);
                    break;
            }
            
            return projects;
        }
    }
}
