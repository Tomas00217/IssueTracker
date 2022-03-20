#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;

namespace IssueTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IssueTrackerContext _context;

        public ProjectsController(IssueTrackerContext context)
        {
            _context = context;
        }

        private Boolean IsNotLogged()
        {
            String idStr = HttpContext.Session.GetString("UserId");

            return idStr == null || idStr.Length <= 0;
        }
        // GET: Projects
        public async Task<IActionResult> Index()
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }

            List<PersonProject> personProject = await _context.PersonProjects.Where(p => p.PersonId == Int32.Parse(HttpContext.Session.GetString("UserId"))).ToListAsync();
            List<Project> projects = new List<Project>();

            personProject.ForEach(p => projects.Add(_context.Projects.FirstOrDefault(x => x.ProjectId == p.ProjectId)));


            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return View("Index");
            }

            return View(project);
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (IsNotLogged())
            {
                return RedirectToAction("Index", "Authorization");
            }


            project.CreatedOn = DateTime.Now;
       
            _context.Projects.Add(project);

            PersonProject personProject = new()
            {
                PersonId = Int32.Parse(HttpContext.Session.GetString("UserId")),
                ProjectId = project.ProjectId,
                Role = 1
            };
            personProject.Project = project;
            personProject.Person = _context.Persons.FirstOrDefault(x => x.PersonId == personProject.PersonId);

            _context.PersonProjects.Add(personProject);


            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Projects/Edit/5
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }
            var projectToUpdate = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == id);
            
/*            if (await TryUpdateModelAsync<Project>(
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

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.ProjectId == id);
        }
    }
}
