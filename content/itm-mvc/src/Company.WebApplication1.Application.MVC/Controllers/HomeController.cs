﻿#if (OrganizationalAuth)
using Microsoft.AspNetCore.Authorization;
#endif
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApplication1.Application.MVC.Controllers
{
#if (OrganizationalAuth)
    [Authorize]
#endif
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

#if (OrganizationalAuth)
        [AllowAnonymous]
#endif
        public IActionResult Error()
        {
            return View();
        }
    }
}
