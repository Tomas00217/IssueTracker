using AspNetCoreHero.ToastNotification.Abstractions;
using IssueTracker.Models;
using IssueTracker.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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

        public async Task<IActionResult> Index(string searchString, string sortOrder, string sortType, 
            IssueStatus state, IssuePriority priority, int pageNumber = 1, int projectId = -1)
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
                issues = issues.Where(i => i.Summary.ToLower().Contains(searchString.ToLower()));
            }

            ViewData["SummarySort"] = String.IsNullOrEmpty(sortOrder) ? "SummaryDesc" : "";
            ViewData["ProjectSort"] = sortOrder == "ProjectSort" ? "ProjectDesc" : "ProjectSort";
            ViewData["StateSort"] = sortOrder == "StateSort" ? "StateDesc" : "StateSort";
            ViewData["PrioritySort"] = sortOrder == "PrioritySort" ? "PriorityDesc" : "PrioritySort";

            issues = Utils.Issues.IssueSorts.sortByOrder(issues, sortOrder);

            ViewData["MyIssues"] = String.IsNullOrEmpty(sortType) ? "MyIssues" : "";
            ViewData["Priority"] = sortType != "Priority" ? "Priority" : "Priority";
            ViewData["State"] = sortType != "State" ? "State" : "State";

            issues = Utils.Issues.IssueSorts.sortByType(issues, sortType, state, priority, userId);

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
            var comments = _context?.Comments?.Where(c => c.IssueId == issue.Id).OrderByDescending(c => c.EditedOn).ToList();

            foreach (var pp in personProjects)
            {
                pp.Person = _context?.Persons?.FirstOrDefault(p => p.PersonId == pp.PersonId);
            }

            var userId = GetUserId();

            issueDetails.issue = issue;
            issueDetails.personProjects = personProjects;
            issueDetails.currentUser = _context?.Persons?.FirstOrDefault(p => p.PersonId == userId);
            issueDetails.comments = comments;

            return View(issueDetails);
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

            ViewBag.MyStatesEnum = ConvertToEnum("states");
            ViewBag.MyPrioritiesEnum = ConvertToEnum("priority");

            return View(issue);
        }

        // POST: Issue/Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                _notyf.Error("Issue does not exist");
                return RedirectToAction(nameof(Index));
            }
            var issue = await _context.Issues.FirstOrDefaultAsync(p => p.Id == id);

            if (issue == null)
            {
                _notyf.Error("Issue does not exist");
                return RedirectToAction(nameof(Index));
            }

            if (await TryUpdateModelAsync<Issue>(
                issue,
                "",
                i => i.Summary, i => i.State, i => i.Priority, i => i.TargetResolutionDate, i => i.ActualResolutionDate, i => i.Description))
            {
                
                try
                {
                    await _context.SaveChangesAsync();
                    _notyf.Success("Issue sucessfuly edited");
                    return RedirectToAction("Details", new {id = issue.Id});
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(issue);
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

        private List<ConvertEnum> ConvertToEnum(string type)
        {
            var myStates = new List<ConvertEnum>();
            
            if (type == "priority")
            {
                foreach (IssuePriority lang in Enum.GetValues(typeof(IssuePriority)))
                    myStates.Add(new ConvertEnum
                    {
                        Value = (int)lang,
                        Text = lang.ToString()
                    });
            } else
            {
                foreach (IssueStatus lang in Enum.GetValues(typeof(IssueStatus)))
                    myStates.Add(new ConvertEnum
                    {
                        Value = (int)lang,
                        Text = lang.ToString()
                    });
            }
            
            return myStates;
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

        public IActionResult AddComment(int id, string comment)
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

            var userId = GetUserId();

            var commentToAdd = new Comment
            {
                IssueId = id,
                Description = comment,
                PersonId = userId,
                EditedOn = DateTime.Now
            };

            _context.Comments.Add(commentToAdd);
            _context.SaveChanges();

            _notyf.Success("Comment added");

            return RedirectToAction("Details", new { id = id });
        }

        public async Task<IActionResult> EditComment(int id, string description)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var comment = _context?.Comments?.Find(id);
            if (comment == null)
            {
                _notyf.Error("Comment not found");
                return RedirectToAction(nameof(Index));
            }

            comment.Description = description;
            comment.EditedOn = DateTime.Now;
            try
            {
                await _context.SaveChangesAsync();
                _notyf.Success("Comment sucessfuly edited");
                return RedirectToAction("Details", new { id = comment.IssueId });
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
            }

            _notyf.Error("Comment is unable to be edited");
            return RedirectToAction("Details", new { id = comment.IssueId });
        }
        public IActionResult DeleteComment(int id)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            var comment = _context?.Comments?.Find(id);
            if (comment == null)
            {
                _notyf.Error("Comment not found");
                return RedirectToAction(nameof(Index));
            }

            var issue = _context?.Issues?.FirstOrDefault(i => i.Id == comment.IssueId);

            var userId = GetUserId();

            if (userId != issue.AsigneeId && userId != issue.CreatorId)
            {
                _notyf.Error("You cannot delete comment from this issue");
                return RedirectToAction("Details", new { id = comment.IssueId });
            }

            _context.Comments.Remove(comment);
            _context.SaveChanges();

            _notyf.Success("Comment deleted");

            return RedirectToAction("Details", new { id = comment.IssueId });
        }
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? -1;
        }
    }
}
