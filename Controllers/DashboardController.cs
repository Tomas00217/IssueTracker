﻿using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
