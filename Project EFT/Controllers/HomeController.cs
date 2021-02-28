using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Project_EFT.Data_Classes;
using Project_EFT.Database;
using Project_EFT.Models;

namespace Project_EFT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // attempts to locate cshtml file with name Index in Home folder and Shared folder
        public IActionResult Index()
        {
            // TODO conditional formatting for logged in users
            return View();
        }

        [HttpPost]
        public IActionResult login()
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];

            StandardUser user = DBConnector.StandardUserLogin(username, password);
            if (user.Id != 0)
            {
                // user props
                ViewData["UserStatus"] = "Standard User";
                HttpContext.Session.SetComplexObject("userInfo", user);
                return RedirectToAction("Index");
            }

            Admin admin = DBConnector.AdminLogin(username, password);
            if (admin.Id != 0)
            {
                // admin props
                ViewData["UserStatus"] = "Admin";
                HttpContext.Session.SetComplexObject("adminInfo", admin);

                // redirect to /, not /Home/login, needs session
                return RedirectToAction("Index");
            }
            else 
            {
                // no user found
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult DeleteUserAccount() 
        {
            string username = Request.Form["username"];
            DBConnector.DeleteUser(username);
            return RedirectToAction("Index");
        }

        public IActionResult ProblemList() 
        {

            // fetch problems from DB and render in view
            ViewData["problems"] = DBConnector.GetProblemsList();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
