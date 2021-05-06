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
    /// <summary>Handles GETs and POSTs for: <br/>
    /// Logging in (POST) and out (GET).
    /// Deleting an account as an admin (POST).
    /// Retrieving and formatting the home page (GET).
    /// Retrieving the Problem List page (GET).
    /// Retrieving the Cipher List page (GET).
    /// Handling invalid subURL requests (GET).
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>If the user is logged in as a <see cref="StandardUser"/>, updates the user's ranking. Otherwise, it only...</summary>
        /// <returns>The home page.</returns>
        public IActionResult Index()
        {
            if (HttpContext.Session.ContainsKey("userInfo")) 
            {
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                user.UpdateRanking();
                HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
            }
            return View();
        }

        /// <summary>Logs the user out by removing all data currently in the session, and...</summary>
        /// <returns>The home page.</returns>
        [HttpGet]
        public IActionResult logout() 
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>Attempts to log the <see cref="User"/> in with the given information. <br/>
        /// If the username or password are invalid, or no user with those credentials exists, an error message is generated to be displayed in the response.<br/>
        /// Otherwise, the user information retrieved from the DB is stored in the session. <br/>
        /// In either case, this...</summary>
        /// <returns>The home page.</returns>
        [HttpPost]
        public IActionResult login()
        {
            string username = Request.Form["username"];
            username = username.Trim();
            string password = Request.Form["password"];

            bool formatErrorExists = false;

            if (HttpContext.Session.ContainsKey("userInfo") || HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index");

            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType)) 
            {
                HttpContext.Session.SetString("usernameFormatError", InformationValidator.InvalidUsernameString);
                formatErrorExists = true;
            }
            if (!InformationValidator.VerifyInformation(password, InformationValidator.PasswordType))
            {
                HttpContext.Session.SetString("passwordFormatError", InformationValidator.InvalidPasswordString);
                formatErrorExists = true;
            }

            if (!formatErrorExists)
            {
                StandardUser user = DBConnector.StandardUserLogin(username, password);
                if (user != null && user.Submissions != null)
                {
                    HttpContext.Session.SetComplexObject("userInfo", user);
                    return RedirectToAction("Index");
                }

                Admin admin = DBConnector.AdminLogin(username, password);
                if (admin != null)
                {
                    admin.Submissions = DBConnector.GetAdminSubmissionsByID(admin.Id);
                    if (admin.Submissions != null) HttpContext.Session.SetComplexObject("adminInfo", admin);
                    else HttpContext.Session.SetString("errorMessage", "Found admin information but could not load submissions. Please try again.");
                    return RedirectToAction("Index");
                }
                HttpContext.Session.SetString("errorMessage", "No user exists with those credentials.");
            }
            return RedirectToAction("Index");
        }

        /// <summary>Attempts to delete the <see cref="StandardUser"/> with the given information. <br/>
        /// If the user is not logged in as an admin, the user is redirected to the home page. <br/>
        /// If the submitted username is invalid, or does not belong to a standard user, an error message is generated to be displayed in the response. <br/>
        /// Otherwise, the given username's user information is removed from the DB. <br/>
        /// In any case, this...</summary>
        /// <returns>The home page.</returns>
        [HttpPost]
        public IActionResult DeleteUserAccount() 
        {
            if (!HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index");
            string username = Request.Form["username"];
            username = username.Trim();
            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType))
            {
                HttpContext.Session.SetString("errorMessage", InformationValidator.InvalidUsernameString);
            }
            else if (DBConnector.DoesUsernameExist(username))
            {
                if (DBConnector.DeleteUser(username)) {
                    Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                    Submission deleteUserSubmission = new Submission("Deleted account with username: " + username, DateTime.Now, admin.Id);
                    if (DBConnector.InsertNewAdminSubmission(deleteUserSubmission))
                    {
                        admin.Submissions.Add(deleteUserSubmission);
                        HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                        HttpContext.Session.SetString("successMessage", "The account was successfully deleted.");
                    }
                    else HttpContext.Session.SetString("errorMessage", "The account was deleted, but the deletion was not recorded as a submission.");
                }
            else
            {
                HttpContext.Session.SetString("errorMessage", "An account was found, but was not deleted. Please try again.");
                HttpContext.Session.SetString("username", username);
            }
            }
            else {
                HttpContext.Session.SetString("errorMessage", "No account with that username exists.");
                HttpContext.Session.SetString("username", username);
            }

            return RedirectToAction("Index");
        }

        /// <summary>Accesses the Problem List page.</summary>
        /// <returns>The Problem List page.</returns>
        public IActionResult ProblemList() 
        {
            return View();
        }

        /// <summary>Accesses the Cipher List page.</summary>
        /// <returns>The Cipher List page.</returns>
        public IActionResult CipherList()
        {

            // fetch problems from DB and render in view
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>Handles all GETs for URL's without mappings (invalid URL's).</summary>
        /// <param name="code">May be the error code for the given error...</param>
        /// <returns>The home page.</returns>
        [Route("/Home/HandleError/{code:int}")]
        public IActionResult Error(int code)
        {
            return RedirectToAction("Index");
        }
    }
}
