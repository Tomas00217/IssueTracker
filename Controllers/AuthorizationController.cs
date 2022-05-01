#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;
using System.Security.Cryptography;
using System.Text;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace IssueTracker.Controllers
{
    public class AuthorizationController : Controller
    {

        private readonly IssueTrackerContext _context;
        private readonly INotyfService _notyf;

        public AuthorizationController(IssueTrackerContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
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

        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Person person, string passwd)
        {
            if (_context.Persons.Any(p => p.Email.Equals(person.Email)))
            {
                _notyf.Error("Email is already in use.");
                return View("Register");
            }

            if (!person.Password.Equals(passwd))
            {
                _notyf.Error("Passwords did not match.");
                return View("Register");
            }

            person.Password = ToSHA512(person.Password);
            _context.Persons.Add(person); 
            await _context.SaveChangesAsync();

            _notyf.Success("Registration sucessful.");
            return View("Index");
        }

        public async Task<IActionResult> Login(Person person)
        {
            Person user = await _context.Persons
                .FirstOrDefaultAsync(u => u.Email.Equals(person.Email));

            if (user == null)
            {
                _notyf.Error("Account with this email does not exist.");
                return View("Index");
            }

            if (ToSHA512(person.Password).Equals(user.Password))
            {
                _notyf.Success("Logged in sucessfully.");
                HttpContext.Session.SetInt32("UserId", user.PersonId);
            } else
            {
                _notyf.Error("Wrong password.");
                return View("Index");
            }

            
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            Console.WriteLine(ToSHA512("adminko123"));
            HttpContext.Session?.Clear();
            _notyf.Success("Logged out sucessfully.");
            return View("Index");
        }

    }
}
