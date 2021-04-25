using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_EFT.Data_Classes;
using Project_EFT.Ciphers;
using Project_EFT.Models;
using Project_EFT.Ciphers.Options;
using Project_EFT.Database;

namespace Project_EFT.Controllers
{
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult search() 
        {
            string queryString = Request.Form["searchCriteria"];
            HttpContext.Session.SetComplexObject<List<StandardUser>>("searchResults", DBConnector.SearchForUser(queryString));
            return RedirectToAction("users");
        }

        public IActionResult users(string username)
        {
            HttpContext.Session.Remove("userToView");
            if (username == null || username == "") 
            {
                List<StandardUser> top5 = DBConnector.GetTopFiveUsers();
                HttpContext.Session.SetComplexObject<List<StandardUser>>("top5", top5);
                return View();
            }
            StandardUser user = DBConnector.GetUserProfileInformationByUsername(username);
            HttpContext.Session.SetComplexObject<StandardUser>("userToView", user);
            return View();
        }

        public IActionResult user(string username) 
        {
            HttpContext.Session.Remove("userToView");
            if (username == null || username == "") return RedirectToAction("Index", "Home");
            StandardUser user = DBConnector.GetUserProfileInformationByUsername(username);
            if (user != null) HttpContext.Session.SetComplexObject<StandardUser>("userToView", user);
            else return RedirectToAction("users");
            return View("users");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
