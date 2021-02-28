using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Project_EFT.Data_Classes;
using Project_EFT.Database;

namespace Project_EFT.Controllers
{
    public class SignupController : Controller
    {
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitSignup() 
        {
            string username = Request.Form["username"];
            string email = Request.Form["email"];
            string password = Request.Form["password"];
            Debug.WriteLine(username);
            Debug.WriteLine(email);
            Debug.WriteLine(password);
            StandardUser newUser = new StandardUser(username, password, email);
            // TODO conditionals
            DBConnector.InsertNewUser(newUser);

            HttpContext.Session.SetComplexObject("userInfo", newUser);

            return RedirectToAction("Index");
        }
    }
}
