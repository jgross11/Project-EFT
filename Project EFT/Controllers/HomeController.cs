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

        [HttpGet]
        public IActionResult logout() 
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult login()
        {
            string username = Request.Form["username"];
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
                if (user.Id != 0)
                {
                    HttpContext.Session.SetComplexObject("userInfo", user);
                    return RedirectToAction("Index");
                }

                Admin admin = DBConnector.AdminLogin(username, password);
                if (admin.Id != 0)
                {
                    admin.Submissions = DBConnector.GetAdminSubmissionsByID(admin.Id);
                    HttpContext.Session.SetComplexObject("adminInfo", admin);
                    return RedirectToAction("Index");
                }
                HttpContext.Session.SetString("errorMessage", "No user exists with those credentials.");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteUserAccount() 
        {
            if (!HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index");
            string username = Request.Form["username"];
            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType))
            {
                HttpContext.Session.SetString("errorMessage", InformationValidator.InvalidUsernameString);
            }
            else if (DBConnector.DoesUsernameExist(username))
            {
                if (DBConnector.DeleteUser(username)) {
                    Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                    Submission deleteUserSubmission = new Submission("Deleted account with username: " + username, DateTime.Now, admin.Id);
                    admin.Submissions.Add(deleteUserSubmission);
                    DBConnector.InsertNewAdminSubmission(deleteUserSubmission);
                    HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                    HttpContext.Session.SetString("successMessage", "The account was successfully deleted.");
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

        public IActionResult ProblemList() 
        {
            return View();
        }

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

        [Route("/Home/HandleError/{code:int}")]
        public IActionResult Error(int code)
        {
            return RedirectToAction("Index");
        }
    }
}
