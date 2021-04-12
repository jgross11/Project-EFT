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
    public class RecoveryController : Controller
    {
        public IActionResult RecoverInfo()
        {
            return View();
        }


        [HttpPost]
        public IActionResult forgotPassword()
        {
            string username = Request.Form["username"];
            string passwordPlain = InformationValidator.GenerateTemporaryPassword();
            string passwordHash = InformationValidator.MD5Hash(passwordPlain);

            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType))
            {
                HttpContext.Session.SetString("usernameErrorMessage", InformationValidator.InvalidUsernameString);
            }
            else
            {
                StandardUser user = DBConnector.GetStandardUserByUsername(username);
                if (user.Id != 0)
                {
                    if (DBConnector.UpdatePassword(user, passwordHash) == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                    {
                        user.Password = passwordPlain;
                        if (Mailer.SendPasswordRecoveryEmail(user))
                        {
                            HttpContext.Session.SetString("passwordResetSuccess", "An email containing your new password has been sent to the email associated with this account.");
                        }
                        else
                        {
                            HttpContext.Session.SetString("passwordResetError", "An error occurred when sending your recovery email. Please try again.");
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("passwordResetError", "An error occurred when resetting your password. Please try again.");
                    }
                }
                else
                {
                    HttpContext.Session.SetString("passwordResetError", "Unable to find an account with that username. Please try again.");
                }
            }

            return RedirectToAction("RecoverInfo");
        }

        [HttpPost]
        public IActionResult forgotUsername()
        {
            string email = Request.Form["email"];
            string passwordPlain = InformationValidator.GenerateTemporaryPassword();
            string passwordHash = InformationValidator.MD5Hash(passwordPlain);

            if (!InformationValidator.VerifyInformation(email, InformationValidator.EmailType))
            {
                HttpContext.Session.SetString("emailErrorMessage", InformationValidator.InvalidEmailString);
            }
            else
            {
                StandardUser user = DBConnector.GetStandardUserByEmail(email);
                if (user.Id != 0)
                {
                    if (DBConnector.UpdatePassword(user, passwordHash) == DBConnector.CREDENTIAL_CHANGE_SUCCESS)
                    {
                        user.Password = passwordPlain;
                        if (Mailer.SendAccountRecoveryEmail(user))
                        {
                            HttpContext.Session.SetString("accountResetSuccess", "An email containing your account information has been sent.");
                        }
                        else
                        {
                            HttpContext.Session.SetString("accountResetError", "An error occurred when sending your recovery email. Please try again.");
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("accountResetError", "An error occurred when resetting your account information. Please try again.");
                    }
                }
                else
                {
                    HttpContext.Session.SetString("accountResetError", "Unable to find an account with that email. Please try again.");
                }
            }
            return RedirectToAction("RecoverInfo");
        }
    }
}
