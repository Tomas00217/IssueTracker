using AspNetCoreHero.ToastNotification.Abstractions;
using IssueTracker.Models;
using IssueTracker.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;

namespace IssueTracker.Controllers
{
    public class IssuesController : Controller
    {
        private readonly IssueTrackerContext _context;
        private readonly INotyfService _notyf;

        public IssuesController(IssueTrackerContext context, INotyfService notyf)
        {
            _notyf = notyf;
            _context = context;
        }
        private Boolean IsNotLogged()
        {
            int? id = HttpContext.Session.GetInt32("UserId");

            return id == null || id < 0;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder, string sortType, IssueStatus state, IssuePriority priority, int pageNumber = 1, int projectId = -1)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;

            var personProjectsIds = projectId < 0 ? 
                _context?.PersonProjects?.Where(p => p.PersonId == userId).Select(p => p.ProjectId) : 
                _context?.PersonProjects?.Where(p => p.PersonId == userId && p.ProjectId == projectId).Select(p => p.ProjectId);

            var issues = from m in _context?.Issues?.Where(i => personProjectsIds.Contains(i.ProjectId))
                .Include(i => i.Creator)
                .Include(i => i.Project) select m;

            foreach (var issue in issues)
            {
                issue.Asignee = _context?.PersonProjects?
                    .Where(p => p.ProjectId == issue.ProjectId && p.PersonId == issue.AsigneeId)?
                    .Select(p => p.Person)?
                    .FirstOrDefault();
            }
            
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
            ViewData["Priority"] = sortType != "Priority" ? "Priority" : "Priority";
            ViewData["State"] = sortType != "State" ? "State" : "State";

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
                    issues = issues.OrderBy(i => i.Summary);
                    break;
            }

            return View(await PaginatedList<Issue>.CreateAsync(issues, pageNumber, 8));
        }

        // GET
        public IActionResult Create()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var userId = GetUserId();

            var projects = _context.GetUserProjects(userId);
        
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

        public IActionResult Details(int id, IssueDetails issueDetails)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var issue = _context?.Issues?.FirstOrDefault(i => i.Id == id);

            if (issue == null)
            {
                _notyf.Error("Issue not found");
                return RedirectToAction(nameof(Index));
            }

            issue.Asignee = _context?.Persons?.FirstOrDefault(p => p.PersonId == issue.AsigneeId);
            issue.Creator = _context?.Persons?.FirstOrDefault(p => p.PersonId == issue.CreatorId);

            var personProjects = _context?.PersonProjects?.Where(proj => proj.ProjectId == issue.ProjectId).ToList();

            foreach (var pp in personProjects)
            {
                pp.Person = _context?.Persons?.FirstOrDefault(p => p.PersonId == pp.PersonId);
            }

            var userId = GetUserId();

            issueDetails.issue = issue;
            issueDetails.personProjects = personProjects;
            issueDetails.currentUser = _context?.Persons?.FirstOrDefault(p => p.PersonId == userId);

            return View(issueDetails);
        }

        public IActionResult AssignPerson(string email, int id)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var issue = _context?.Issues?.FirstOrDefault(i => i.Id == id);
            

            if (issue == null)
            {
                _notyf.Error("Issue not found");
                return RedirectToAction(nameof(Index));
            }
            
            var person = _context?.Persons?.FirstOrDefault(p => p.Email == email);
            var personProjects = _context?.PersonProjects?.Where(proj => proj.ProjectId == issue.ProjectId).Select(proj => proj.PersonId).ToList();
            if (person == null || personProjects == null)
            {
                _notyf.Error("Person not found");
                return RedirectToAction("Details", new { id = id });
            }
            
            if (!personProjects.Contains(person.PersonId))
            {
                _notyf.Error("Person is not member of this project");
                return RedirectToAction("Details", new { id = id });
            }

            issue.AsigneeId = person.PersonId;
            issue.Asignee = person;

            _context?.SaveChanges();
            _notyf.Success("Person assigned");
            
            return RedirectToAction("Details", new { id = id });
        }

        public IActionResult MarkAs(int id, IssueStatus state)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var issue = _context?.Issues?.FirstOrDefault(i => i.Id == id);
            if (state == IssueStatus.Resolved)
            {
                issue.ActualResolutionDate = DateTime.Now;
            }
            issue.State = state;

            _context.SaveChanges();
            _notyf.Success("Issue state updated");

            return RedirectToAction("Details", new { id = id });
        }

        // GET: Issue/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _notyf.Error("Invalid action");
                return RedirectToAction("Index");
            }

            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
            {
                _notyf.Error("Issue not found");
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;

            if (userId != issue.AsigneeId && userId != issue.CreatorId)
            {
                _notyf.Error("You cannot edit this issue");
                return RedirectToAction("Index");
            }

            return View(issue);
        }

        // POST: Issue/Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(int? id)
        {
            return View();
        }

        // POST: Issues
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var issue = _context.Issues.Find(id);
            _context.Issues.Remove(issue);
            _context.SaveChanges();

            _notyf.Success("Issue sucessfuly deleted");
            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? -1;
        }
    }
}
