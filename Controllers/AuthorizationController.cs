﻿#nullable disable
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Person person, string passwd)
        {
            if (_context.Persons.Any(p => p.Email.Equals(person.Email)))
            {
                _notyf.Error("Email is already in use.");
                return View("Index");
            }

            if (!person.Password.Equals(passwd))
            {
                _notyf.Error("Passwords didn't match.");
                return View("Index");
            }

            person.Password = ToSHA512(person.Password);
            _context.Persons.Add(person); 
            await _context.SaveChangesAsync();

            _notyf.Success("Registration sucessfull.");
            return View("Index");
        }

        public async Task<IActionResult> Login(Person person)
        {
            Person user = await _context.Persons
                .FirstOrDefaultAsync(u => u.Email.Equals(person.Email));

            if (user == null)
            {
                _notyf.Error("Unknown email.");
                return View("Index");
            }

            if (ToSHA512(person.Password).Equals(user.Password))
            {
                _notyf.Success("Logged in sucessfully.");
                HttpContext.Session.SetString("UserId", user.PersonId.ToString());
            } else
            {
                _notyf.Error("Wrong password.");
                return View("Index");
            }

            
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session?.Clear();
            _notyf.Success("Logged out sucessfully.");
            return View("Index");
        }

    }
}
