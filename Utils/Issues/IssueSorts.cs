using IssueTracker.Models;
namespace IssueTracker.Utils.Issues
{
    public static class IssueSorts 
    {
        
        public static IQueryable<Issue> sortByOrder(IQueryable<Issue> issues,string sortOrder)
        {
            switch (sortOrder)
            {
                case "SummaryDesc":
                    issues = issues.OrderByDescending(i => i.Summary);
                    break;
                case "ProjectSort":
                    issues = issues.OrderBy(i => i.Project.Name);
                    break;
                case "ProjectDesc":
                    issues = issues.OrderByDescending(i => i.Project.Name);
                    break;
                case "StateSort":
                    issues = issues.OrderBy(i => i.State);
                    break;
                case "StateDesc":
                    issues = issues.OrderByDescending(i => i.State);
                    break;
                case "PrioritySort":
                    issues = issues.OrderBy(i => i.Priority);
                    break;
                case "PriorityDesc":
                    issues = issues.OrderByDescending(i => i.Priority);
                    break;
                default:
                    issues = issues.OrderBy(i => i.Summary);
                    break;
            }

            return issues;
        }

        public static IQueryable<Issue> sortByType(IQueryable<Issue> issues, string sortType, IssueStatus state, IssuePriority priority, int userId)
        {   
            
            switch (sortType)
            {
                case "MyIssues":
                    issues = issues.Where(i => i.AsigneeId == userId || i.CreatorId == userId);
                    break;
                case "Priority":
                    issues = issues.Where(i => i.Priority == priority);
                    break;
                case "State":
                    issues = issues.Where(i => i.State == state);
                    break;
                default:
                    break;
            }

            return issues;
        }
    }
}
