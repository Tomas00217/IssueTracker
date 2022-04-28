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
                return RedirectToAction("Index", "Authorization");
            }

            var person = _context.Persons
                .FirstOrDefault(m => m.PersonId == id);
            if (person == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            return View("Index", person);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            var person = await _context.Persons.FindAsync(id);
            if (person == null)
            {
                return RedirectToAction("Index", "Authorization");
            }
            return View(person);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Authorization");
            }
            var personToUpdate = await _context.Persons.FirstOrDefaultAsync(p => p.PersonId == id);

            if (await TryUpdateModelAsync<Person>(
                personToUpdate,
                "",
                p => p.FirstName, p => p.SecondName))
            {

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Profile));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

            return View(personToUpdate);
        }
        
        // POST: Persons/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Persons.Find(id);
            _context.Persons.Remove(user);
            _context.SaveChanges();

            _notyf.Success("Account deleted");
            return RedirectToAction("Index", "Authorization");
        }

    }
}
