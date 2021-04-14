using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_EFT.Data_Classes;
using Project_EFT.Database;

namespace Project_EFT.Controllers
{
    public class EditInfoController : Controller
    {
        public IActionResult editInfo()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || HttpContext.Session.ContainsKey("adminInfo"))
            {
                return View();
            }
            else {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult editUsername() 
        {
            if (!HttpContext.Session.ContainsKey("userInfo") && !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");

            string username = Request.Form["username"];

            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType)) 
            {
                HttpContext.Session.SetString("usernameError", InformationValidator.InvalidUsernameString);
            }
            // TODO fix specific adminInfo / userInfo session being used, should be one base user class that is cast appropriately
            else if (HttpContext.Session.ContainsKey("userInfo"))
            {
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                int result = DBConnector.TryUpdateUsername(user, username);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("usernameSuccess", "Username changed successfully.");
                    user.Username = username;
                    HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
                }
                else if (result == DBConnector.CREDENTIAL_TAKEN)
                {
                    HttpContext.Session.SetString("usernameError", "That username belongs to another account.");
                }
                else
                {
                    HttpContext.Session.SetString("usernameError", "Unable to update username, please try again.");
                }
            }
            else if (HttpContext.Session.ContainsKey("adminInfo"))
            {
                Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                int result = DBConnector.TryUpdateUsername(admin, username);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("usernameSuccess", "Username changed successfully.");
                    admin.Username = username;
                    HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                }
                else if (result == DBConnector.CREDENTIAL_TAKEN)
                {
                    HttpContext.Session.SetString("usernameError", "That username belongs to another account.");
                }
                else
                {
                    HttpContext.Session.SetString("usernameError", "Unable to update username, please try again.");
                }
            }
            return RedirectToAction("editInfo", "EditInfo");
        }

        [HttpPost]
        public IActionResult editEmail()
        {
            if (!HttpContext.Session.ContainsKey("userInfo") && !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");

            string email = Request.Form["email"];

            if (!InformationValidator.VerifyInformation(email, InformationValidator.EmailType))
            {
                HttpContext.Session.SetString("emailError", InformationValidator.InvalidEmailString);
            }

            // TODO fix specific adminInfo / userInfo session being used, should be one base user class that is cast appropriately
            else if (HttpContext.Session.ContainsKey("userInfo"))
            {
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                int result = DBConnector.TryUpdateEmail(user, email);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("emailSuccess", "Email changed successfully.");
                    user.Email = email;
                    HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
                }
                else if (result == DBConnector.CREDENTIAL_TAKEN)
                {
                    HttpContext.Session.SetString("emailError", "That email belongs to another account.");
                }
                else
                {
                    HttpContext.Session.SetString("emailError", "Unable to update email, please try again.");
                }
            }
            else if (HttpContext.Session.ContainsKey("adminInfo"))
            {
                Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                int result = DBConnector.TryUpdateEmail(admin, email);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("emailSuccess", "Email changed successfully.");
                    admin.Email = email;
                    HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                }
                else if (result == DBConnector.CREDENTIAL_TAKEN)
                {
                    HttpContext.Session.SetString("emailError", "That email belongs to another account.");
                }
                else
                {
                    HttpContext.Session.SetString("emailError", "Unable to update email, please try again.");
                }
            }
            return RedirectToAction("editInfo", "EditInfo");
        }

        [HttpPost]
        public IActionResult editPassword()
        {
            if (!HttpContext.Session.ContainsKey("userInfo") && !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");

            string password = Request.Form["password"];

            if (!InformationValidator.VerifyInformation(password, InformationValidator.PasswordType))
            {
                HttpContext.Session.SetString("passwordError", InformationValidator.InvalidPasswordString);
            }

            // TODO fix specific adminInfo / userInfo session being used, should be one base user class that is cast appropriately
            else if (HttpContext.Session.ContainsKey("userInfo"))
            {
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                int result = DBConnector.UpdatePassword(user, password);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("passwordSuccess", "Password changed successfully.");
                    user.Password = password;
                    HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
                }
                else
                {
                    HttpContext.Session.SetString("passwordError", "Unable to update password, please try again.");
                }
            }
            else if (HttpContext.Session.ContainsKey("adminInfo"))
            {
                Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                int result = DBConnector.UpdatePassword(admin, password);
                if (result == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                {
                    HttpContext.Session.SetString("passwordSuccess", "Password changed successfully.");
                    admin.Password = password;
                    HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                }
                else
                {
                    HttpContext.Session.SetString("passwordError", "Unable to update password, please try again.");
                }
            }
            return RedirectToAction("editInfo", "EditInfo");
        }
    }
}
