#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace IssueTracker.Controllers
{
    public class PersonsController : Controller
    {
        private readonly IssueTrackerContext _context;
        private readonly INotyfService _notyf;

        public PersonsController(IssueTrackerContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            int? id = HttpContext.Session.GetInt32("UserId");

            if (id == null)
            {
                _notyf.Error("You need to be logged in to view your profile.");
                return RedirectToAction("Index", "Authorization");
            }

            var person = _context.Persons
                .FirstOrDefault(p => p.PersonId == id);
            
            if (person == null)
            {
                _notyf.Error("Person not found.");
                return RedirectToAction("Index", "Authorization");
            }

            return View("Index", person);
        }

        // GET: Persons/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _notyf.Error("You need to be logged in to edit your profile.");
                return RedirectToAction("Index", "Authorization");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (id != userId)
            {
                _notyf.Error("Access denied.");
                return RedirectToAction("Profile");
            }

            var person = await _context.Persons
                .FindAsync(id);
            
            if (person == null)
            {
                _notyf.Error("Person not found.");
                return RedirectToAction("Index", "Authorization");
            }
            
            return View(person);
        }

        // POST: Persons/Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                _notyf.Error("You need to be logged in to edit your profile.");
                return RedirectToAction("Index", "Authorization");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (id != userId)
            {
                _notyf.Error("Access denied.");
                return RedirectToAction("Profile");
            }

            var personToUpdate = await _context.Persons
                .FirstOrDefaultAsync(p => p.PersonId == id);

            // Update only the changed fields
            if (await TryUpdateModelAsync<Person>(
                personToUpdate,
                "",
                p => p.FirstName, p => p.SecondName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    
                    _notyf.Success("Profile updated successfully");
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

            _notyf.Error("Updating profile failed");
            return View(personToUpdate);
        }
        
        // POST: Persons/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Persons
                .Find(id);
            
            _context.Persons.Remove(user);
            _context.SaveChanges();

            _notyf.Success("Account deleted");
            return RedirectToAction("Index", "Authorization");
        }

    }
}