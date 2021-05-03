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
    /// <summary>Handles GETs and POSTs for: <br/>
    /// - Accessing the Recover Information Page (GET).
    /// - Recovering an account's username or password (POST).
    /// </summary>
    public class RecoveryController : Controller
    {
        /// <summary>Retreives the Recover Information page.</summary>
        /// <returns>The Recover Information page.</returns>
        public IActionResult RecoverInfo()
        {
            return View();
        }

        /// <summary>Attempts to reset a user's password based off the given username. <br/>
        /// If the username is invalid, generates an error message for displaying in the response. <br/>
        /// Otherwise, attempts to set the user's password to a randomly generated one and send a recovery email with the generated password. <br/>
        /// If the password cannot be reset, or the email cannot be sent, an error message is generated for displaying in the response.</summary>
        /// <returns>The Recover Information page, with either error or success message(s).</returns>
        [HttpPost]
        public IActionResult forgotPassword()
        {
            string username = Request.Form["username"];
            username = username.Trim();
            string passwordPlain = InformationValidator.GenerateTemporaryPassword();
            string passwordHash = InformationValidator.MD5Hash(passwordPlain);

            if (!InformationValidator.VerifyInformation(username, InformationValidator.UsernameType))
            {
                HttpContext.Session.SetString("usernameErrorMessage", InformationValidator.InvalidUsernameString);
            }
            else
            {
                StandardUser user = DBConnector.GetStandardUserByUsername(username);
                if (user != null)
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

        /// <summary>Attempts to reset a user's username based off the given email. <br/>
        /// If the email is invalid, generates an error message for displaying in the response. <br/>
        /// Otherwise, attempts to set the user's password to a randomly generated one and send a recovery email with the generated password and username. <br/>
        /// If the password cannot be reset, or the email cannot be sent, an error message is generated for displaying in the response.</summary>
        /// <returns>The Recover Information page, with either error or success message(s).</returns>
        [HttpPost]
        public IActionResult forgotUsername()
        {
            string email = Request.Form["email"];
            email = email.Trim();
            string passwordPlain = InformationValidator.GenerateTemporaryPassword();
            string passwordHash = InformationValidator.MD5Hash(passwordPlain);

            if (!InformationValidator.VerifyInformation(email, InformationValidator.EmailType))
            {
                HttpContext.Session.SetString("emailErrorMessage", InformationValidator.InvalidEmailString);
            }
            else
            {
                StandardUser user = DBConnector.GetStandardUserByEmail(email);
                if (user != null)
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
