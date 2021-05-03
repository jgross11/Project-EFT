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
    /// <summary>Handles GETs and POSTs for: <br/>
    /// - Searching for a user by username (POST).
    /// - Retrieving and displaying a user's profile (GET).
    /// </summary>
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;

        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        /// <summary>Searches for a user with the given information.</summary>
        /// <returns>The Users page, populated with the results of the DB user search query.</returns>
        [HttpPost]
        public IActionResult search() 
        {
            string queryString = Request.Form["searchCriteria"];
            HttpContext.Session.SetComplexObject<List<StandardUser>>("searchResults", DBConnector.SearchForUser(queryString));
            return RedirectToAction("users");
        }

        /// <summary>Attempts to load a user's profile with the given information.<br/>
        /// If the given username is valid, a user exists with the given username, and the user's profile can be retrieved,
        /// the profile will be displayed. Otherwise, the user is redirected to the User search page.
        /// </summary>
        /// <param name="username">The username of the user whose profile is desired.</param>
        /// <returns>The user's profile, if the above criteria are met. Otherwise, the user search page.</returns>
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
            if(user!=null)HttpContext.Session.SetComplexObject<StandardUser>("userToView", user);
            return View();
        }

        /// <summary>Attempts to load a user's profile with the given information.<br/>
        /// If the given username is valid, a user exists with the given username, and the user's profile can be retrieved,
        /// the profile will be displayed. Otherwise, the user is redirected to the User search page.
        /// </summary>
        /// <param name="username">The username of the user whose profile is desired.</param>
        /// <returns>The user's profile, if the above criteria are met. Otherwise, the user search page.</returns>
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
