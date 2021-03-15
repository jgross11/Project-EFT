using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
            if (DBConnector.DoesEmailExist(email))
                HttpContext.Session.SetString("errorMessage", "The submitted email is already associated with an account.");
            else if (DBConnector.DoesUsernameExist(username))
                HttpContext.Session.SetString("errorMessage", "The submitted username is already associated with an account.");
            else
            {
                StandardUser newUser = new StandardUser(username, password, email);
                int result = DBConnector.InsertNewUser(newUser);
                if (result != -1)
                {
                    newUser.Id = result;
                    HttpContext.Session.SetComplexObject("userInfo", newUser);
                    return RedirectToAction("Index", "Home");
                }
                else
                    HttpContext.Session.SetString("errorMessage", "Error creating account. Please try again.");
            }
            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("email", email);

            // TODO fix weird routing (/Signup/signup instead of just /Signup)
            return RedirectToAction("signup", "Signup");
        }
    }
}
