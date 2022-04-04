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

        public IActionResult Index()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            var user = _context?.Persons?.Find(GetUserId());

            var issueTrackerContext = _context?.Issues?.Where(i => i.CreatorId == GetUserId()).Include(i => i.Creator).Include(i => i.Project);
            return View(issueTrackerContext.ToList());
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
