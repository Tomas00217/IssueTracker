#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;
using System.Security.Cryptography;
using System.Text;

namespace IssueTracker.Controllers
{
    public class AuthorizationController : Controller
    {

        private readonly IssueTrackerContext _context;

        public AuthorizationController(IssueTrackerContext context)
        {
            _context = context;
        }

        public string ToSHA512(string input)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha512Hash.ComputeHash(inputBytes);

                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty); ;

                return hash;
            }

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Person person, string passwd)
        {
            /*if (!person.Password.Equals(passwd))
            {
                return View("Index", person);
            }*/
            if (_context.Persons.Any(p => p.Email.Equals(person.Email)))
            {               
                return View("Index", person);
            }

            person.Password = ToSHA512(person.Password);
            _context.Persons.Add(person);
            
            await _context.SaveChangesAsync();

            return View("Index");
        }

        public async Task<IActionResult> Login(Person person)
        {
            Person user = await _context.Persons
                .FirstOrDefaultAsync(u => u.Email.Equals(person.Email));
            if (user == null)
            {
                return NotFound();
            }

            if (ToSHA512(person.Password).Equals(user.Password))
            {
                HttpContext.Session.SetString("UserEmail", user.Email);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session?.Clear();
            return View("Index");
        }

    }
}
