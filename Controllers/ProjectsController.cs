#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;
using IssueTracker.Utils;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace IssueTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IssueTrackerContext _context;
        private readonly INotyfService _notyf;

        public ProjectsController(IssueTrackerContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        private Boolean IsNotLogged()
        {
            int? id = HttpContext.Session.GetInt32("UserId");

            return id == null || id < 0;
        }

        // GET: Projects
        public IActionResult Index()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            
            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;
            var projects = _context.GetUserProjects(userId);

            var projectIds = _context.PersonProjects.Select(proj => proj.ProjectId);
            ViewData["ProjectIds"] = projectIds;

            var issues = _context.Issues.ToList();
            ViewData["Issues"] = issues;

            return View(projects);
        }

        // GET: Projects/Details
        public IActionResult Details(int? id, int userId, ProjectDetails projectDetails)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            if (id == null || userId < 0)
            {
                return RedirectToAction("Index");
            }

            var project = _context.Projects
                .FirstOrDefault(m => m.ProjectId == id);
            if (project == null)
            {
                return View("Index");
            }

            projectDetails.Project = project;
            projectDetails.projectIds = _context.PersonProjects.Select(proj => proj.ProjectId);
            projectDetails.projectLead = _context.PersonProjects.Where(proj => proj.ProjectId == id && proj.Role == ProjectRole.ProjectLead).Select(p => p.Person.Email).SingleOrDefault();
            projectDetails.issues = _context.Issues.ToList();
            projectDetails.userRole = _context.PersonProjects.Where(proj => proj.ProjectId == id && proj.PersonId == userId).Select(pers => pers.Role).SingleOrDefault();
            projectDetails.personProjects = _context.PersonProjects.Where(proj => proj.ProjectId == id);
            projectDetails.allUsers = _context.Persons.ToList();

            return View(projectDetails);
        }

        public IActionResult InviteDeveloper(string email, int projectId)
        {

            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? -1;
            Person userToAdd = _context.Persons.FirstOrDefault(pers => pers.Email == email);

            if (userToAdd == null)
            {
                _notyf.Error("User with email " + email + " does not exist");
                return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
            }

            int userIdToAdd = userToAdd.PersonId;

            if (_context.PersonProjects.Where(pp => pp.PersonId == userIdToAdd && pp.ProjectId == projectId).Any())
            {
                _notyf.Error("User is already part of project");
                return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
            }

            PersonProject personProject = new()
            {
                PersonId = userIdToAdd,
                ProjectId = projectId,
                Role = ProjectRole.Developer
            };

            var project = _context.Projects.FirstOrDefault(proj => proj.ProjectId == projectId);

            personProject.Project = project;
            personProject.Person = _context.Persons.FirstOrDefault(x => x.PersonId == personProject.PersonId);
            _context.PersonProjects.Add(personProject);
            _context.SaveChanges();

            _notyf.Success("User sucessfuly added");
            return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
        }

        public IActionResult SetProjectLead(string email, int projectId)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? -1;

            var project = _context.Projects.FirstOrDefault(proj => proj.ProjectId == projectId);
            var person = _context.Persons.FirstOrDefault(pers => pers.Email == email);

            if (project == null || person == null)
            {
                _notyf.Error("Project or user does not exist");
                return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
            }

            var personProject = _context.PersonProjects.FirstOrDefault(pp => pp.Person.Email == email && pp.ProjectId == projectId);

            if (personProject == null)
            {
                _notyf.Error("User is not part of project");
                return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
            }

            personProject.Role = ProjectRole.ProjectLead;
            _context.SaveChanges();

            _notyf.Success("Project lead sucessfuly set");
            return RedirectToAction("Details", new { id = projectId, userId = currentUserId });
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Project project)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            project.CreatedOn = DateTime.Now;
       
            _context.Projects.Add(project);

            PersonProject personProject = new()
            {
                PersonId = HttpContext.Session.GetInt32("UserId") ?? -1,
                ProjectId = project.ProjectId,
                Role = ProjectRole.Manager
            };

            personProject.Project = project;
            personProject.Person = _context.Persons.FirstOrDefault(x => x.PersonId == personProject.PersonId);

            _context.PersonProjects.Add(personProject);
            _context.SaveChanges();

            _notyf.Success("Project sucessfuly created");
            return RedirectToAction("Index");
        }

        // GET: Projects/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }
            var projectToUpdate = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            
        /*  if (await TryUpdateModelAsync<Project>(
                projectToUpdate,
                "",
                p => p.Name, p => p.StartDate, p => p.TargetEndDate, p => p.ActualEndDate, p => p.ModifiedBy))
            {

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException )
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }*/
            return View(projectToUpdate);
        }

        // POST: Projects/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {   
            var project = _context.Projects.Find(id);
            _context.Projects.Remove(project);
            _context.SaveChanges();

            _notyf.Success("Project sucessfuly deleted");
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
