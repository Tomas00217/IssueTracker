using IssueTracker.Models;
using IssueTracker.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace IssueTracker.Controllers
{
    public class IssuesController : Controller
    {
        private readonly IssueTrackerContext _context;

        public IssuesController(IssueTrackerContext context)
        {
            _context = context;
        }
        private Boolean IsNotLogged()
        {
            int? id = HttpContext.Session.GetInt32("UserId");

            return id == null || id < 0;
        }

        public IActionResult Index(string searchString, string sortOrder, string sortType)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;

            var personProjectsIds = _context?.PersonProjects?.Where(p => p.PersonId == userId).Select(p => p.ProjectId);

            var issues = from m in _context?.Issues?
                .Where(i => personProjectsIds.Contains(i.ProjectId))
                .Include(i => i.Creator).Include(i => i.Project)
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                issues = issues.Where(i => i.Summary.Contains(searchString));
            }

            ViewData["SummarySort"] = String.IsNullOrEmpty(sortOrder) ? "SummaryDesc" : "";
            ViewData["ProjectSort"] = sortOrder == "ProjectSort" ? "ProjectDesc" : "ProjectSort";
            ViewData["StateSort"] = sortOrder == "StateSort" ? "StateDesc" : "StateSort";
            ViewData["PrioritySort"] = sortOrder == "PrioritySort" ? "PriorityDesc" : "PrioritySort";

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

            ViewData["MyIssues"] = String.IsNullOrEmpty(sortType) ? "MyIssues" : "";
            ViewData["PriorityHigh"] = sortType != "PriorityHigh" ? "PriorityHigh" : "";
            ViewData["PriorityMedium"] = sortType != "PriorityMedium" ? "PriorityMedium" : "";
            ViewData["PriorityLow"] = sortType != "PriorityLow" ? "PriorityLow" : "";
            ViewData["StateNew"] = sortType != "StateNew" ? "StateNew" : "";
            ViewData["StateActive"] = sortType != "StateActive" ? "StateActive" : "";
            ViewData["StateResolved"] = sortType != "StateResolved" ? "StateResolved" : "";
            ViewData["StateClosed"] = sortType != "StateClosed" ? "StateClosed" : "";

            switch (sortType)
            {
                case "MyIssues":
                    issues = issues.Where(i => i.AsigneeId == userId || i.CreatorId == userId);
                    break;
                case "PriorityHigh":
                    issues = issues.Where(i => i.Priority == IssuePriority.High);
                    break;
                case "PriorityMedium":
                    issues = issues.Where(i => i.Priority == IssuePriority.Medium);
                    break;
                case "PriorityLow":
                    issues = issues.Where(i => i.Priority == IssuePriority.Low);
                    break;
                case "StateNew":
                    issues = issues.Where(i => i.State == IssueStatus.New);
                    break;
                case "StateActive":
                    issues = issues.Where(i => i.State == IssueStatus.Active);
                    break;
                case "StateResolved":
                    issues = issues.Where(i => i.State == IssueStatus.Resolved);
                    break;
                case "StateClosed":
                    issues = issues.Where(i => i.State == IssueStatus.Closed);
                    break;
                default:
                    issues = issues.OrderBy(i => i.Summary);
                    break;
            }

            return View(issues);
        }

        // GET
        public IActionResult Create()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var userId = GetUserId();

            List<Project> projects = _context.GetUserProjects(userId);
        
            ViewData["ProjectId"] = new SelectList(projects, "ProjectId", "Name");

            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Issue issue)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var userId = GetUserId();
            var creator = _context?.Persons?.FirstOrDefault(p => p.PersonId == userId);

            issue.CreatedOn = DateTime.Now;
            issue.CreatorId = userId;
            issue.State = IssueStatus.New;
            issue.Creator = creator;

            _context?.Add(issue);
            _context?.SaveChanges();

            return RedirectToAction("Index");
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? -1;
        }
    }
}
