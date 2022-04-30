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
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            
            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;
            var projects = from m in _context.GetUserProjects(userId) select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Name.Contains(searchString));
            }

            ViewData["NameSort"] = String.IsNullOrEmpty(sortOrder) ? "NameDesc" : "";
            ViewData["StartDateSort"] = sortOrder == "StartDate" ? "StartDateDesc" : "StartDate";
            ViewData["TargetDateSort"] = sortOrder == "TargetDate" ? "TargetDateDesc" : "TargetDate";
            ViewData["StatusSort"] = sortOrder == "Status" ? "StatusDesc" : "Status";

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

            var projectIds = _context.PersonProjects.Select(proj => proj.ProjectId);
            ViewData["ProjectIds"] = projectIds;

            var issues = _context.Issues.ToList();
            ViewData["Issues"] = issues;

            return View(await PaginatedList<Project>.CreateAsync(projects, pageNumber, 10));
        }

        // GET: Projects/Details
        public async Task<IActionResult> Details(int? id, int userId, ProjectDetails projectDetails, int pageNumber = 1)
        {
            if (IsNotLogged() || userId != HttpContext.Session.GetInt32("UserId"))
            {
                return RedirectToAction("Index", "Authorization");
            }

            var project = _context.Projects
                .FirstOrDefault(m => m.ProjectId == id);
            if (project == null)
            {
                return RedirectToAction("Index");
            }

            projectDetails.Project = project;
            projectDetails.projectIds = _context.PersonProjects.Select(proj => proj.ProjectId);
            projectDetails.projectLead = _context.PersonProjects.Where(proj => proj.ProjectId == id && proj.Role == ProjectRole.ProjectLead).Select(p => p.Person.Email).SingleOrDefault();
            var issues = _context.Issues.Where(i => i.ProjectId == id);
            projectDetails.issues = await PaginatedList<Issue>.CreateAsync(issues, pageNumber, 5);
            projectDetails.userRole = _context.PersonProjects.Where(proj => proj.ProjectId == id && proj.PersonId == userId).Select(pers => pers.Role).SingleOrDefault();
            var personProjects = _context.PersonProjects.Where(proj => proj.ProjectId == id);
            projectDetails.personProjects = await PaginatedList<PersonProject>.CreateAsync(personProjects, pageNumber, 10);
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

            var personProject = _context.PersonProjects.FirstOrDefault(pp => pp.Role == ProjectRole.ProjectLead && pp.ProjectId == projectId);
            
            if (personProject != null)
            {
                personProject.Role = ProjectRole.Developer;
                _context.SaveChanges();
            }

            personProject = _context.PersonProjects.FirstOrDefault(pp => pp.Person.Email == email && pp.ProjectId == projectId);

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

        public IActionResult RemoveFromProject(int userId, ProjectRole currentRole)
        {
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? -1;

            var personProject = _context.PersonProjects.FirstOrDefault(pp => pp.PersonId == userId);

            if (currentRole == ProjectRole.Developer)
            {
                _notyf.Error("Invalid action");
                return RedirectToAction("UserList", new { projectId = personProject.ProjectId});
            }
            
            if (personProject == null)
            {
                _notyf.Error("User is not part of project");
                return RedirectToAction("UserList", new { projectId = personProject.ProjectId});
            } 
            
            if (personProject.Role == ProjectRole.Manager && currentRole != ProjectRole.Manager)
            {
                _notyf.Error("Only manager can remove manager");
                return RedirectToAction("UserList", new { projectId = personProject.ProjectId});
            }
            
            _context.PersonProjects.Remove(personProject);
            _context.SaveChanges();

            _notyf.Success("User sucessfuly removed from project");
            return RedirectToAction("UserList", new { projectId = personProject.ProjectId});
        }

        public async Task<IActionResult> UserList(int projectId, ProjectDetails projectDetails, int pageNumber = 1)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }
            var project = _context.Projects
                .FirstOrDefault(m => m.ProjectId == projectId);
            if (project == null)
            {
                return RedirectToAction("Index");
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? -1;

            projectDetails.Project = project;
            var personProjects = _context.PersonProjects.Where(proj => proj.ProjectId == projectId);
            projectDetails.personProjects = await PaginatedList<PersonProject>.CreateAsync(personProjects, pageNumber, 10);
            projectDetails.userRole = _context.PersonProjects.Where(proj => proj.ProjectId == projectId && proj.PersonId == userId).Select(pers => pers.Role).SingleOrDefault();

            foreach (var pp in projectDetails.personProjects)
            {
                pp.Person = _context.Persons.FirstOrDefault(pers => pers.PersonId == pp.PersonId);
            }

            return View(projectDetails);
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

            var userId = HttpContext.Session.GetInt32("UserId") ?? -1;
            var personProject = _context.PersonProjects.FirstOrDefault(pp => pp.PersonId == userId && pp.ProjectId == id);
            if (personProject == null || personProject.Role != ProjectRole.Manager)
            {
                _notyf.Error("You cannot edit this project");
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // POST: Projects/Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, string email)
        {
            if (id == null)
            {
                _notyf.Error("Project does not exist");
                return RedirectToAction("Index");
            }
            var projectToUpdate = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            
            if (projectToUpdate == null)
            {
                _notyf.Error("Project does not exist");
                return RedirectToAction(nameof(Index));
            }

            if (await TryUpdateModelAsync<Project>(
                projectToUpdate,
                "",
                p => p.Name, p => p.StartDate, p => p.TargetEndDate, p => p.ActualEndDate))
            {

                try
                {
                    await _context.SaveChangesAsync();
                    _notyf.Success("Project sucessfuly edited");
                    return RedirectToAction("Details", new { id = id, userId = HttpContext.Session.GetInt32("UserId") ?? -1 });
                }
                catch (DbUpdateException )
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
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
