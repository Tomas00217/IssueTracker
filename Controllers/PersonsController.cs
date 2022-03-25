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
    public class PersonsController : Controller
    {
        private readonly IssueTrackerContext _context;

        public PersonsController(IssueTrackerContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            String idStr = HttpContext.Session.GetString("UserId");

            if (idStr == null)
            {
                return RedirectToAction("Index", "Authorization");
            }

            int? id = Int32.Parse(idStr);

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

    }
}
