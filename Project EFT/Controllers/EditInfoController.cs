﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project_EFT.Data_Classes;
using Project_EFT.Database;
using FileChecker = System.IO.File; 
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Project_EFT.Controllers
{
    /// <summary>Handles GETs and POSTs for: <br/>
    /// - Editting username, email, password, profile picture, and about information(POSTs).<br/>
    /// - Accessing the edit information page (GET).
    /// </summary>
    public class EditInfoController : Controller
    {
        /// <summary>Determines if the user should be able to access the edit information page.</summary>
        /// <returns>The edit information page, if the user is logged in as a <see cref="StandardUser"/> or <see cref="Admin"/>. Otherwise, the home page.</returns>
        public IActionResult editInfo()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || HttpContext.Session.ContainsKey("adminInfo"))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>Attempts to modify a <see cref="User"/>'s <see cref="User.Username"/> based on the submitted form. <br/>
        /// If the user is not logged in, the user is redirected to the home page. <br/>
        /// If the submitted username is a valid username, and does not belong to another user of the same type, the user's username is updated in the DB. <br/>
        /// If any errors occur, the appropriate error messages are generated for displaying in the response.</summary>
        /// <returns>The edit information page, with either the success message or an error, if the user is logged in. Otherwise, the home page.</returns>
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

        /// <summary>Attempts to modify a <see cref="User"/>'s <see cref="User.Email"/> based on the submitted form. <br/>
        /// If the user is not logged in, the user is redirected to the home page. <br/>
        /// If the submitted email is a valid email, and does not belong to another user of the same type, the user's email is updated in the DB. <br/>
        /// If any errors occur, the appropriate error messages are generated for displaying in the response.</summary>
        /// <returns>The edit information page, with either the success message or an error, if the user is logged in. Otherwise, the home page.</returns>
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

        /// <summary>Attempts to modify a <see cref="User"/>'s <see cref="User.Password"/> based on the submitted form. <br/>
        /// If the user is not logged in, the user is redirected to the home page. <br/>
        /// If the submitted password is a valid password, the user's password is updated in the DB. <br/>
        /// If any errors occur, the appropriate error messages are generated for displaying in the response.</summary>
        /// <returns>The edit information page, with either the success message or an error, if the user is logged in. Otherwise, the home page.</returns>
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

        /// <summary>Attempts to modify a <see cref="StandardUser"/>'s profile picture based on the submitted form. <br/>
        /// If the user is not logged in, the user is redirected to the home page. <br/>
        /// If the submitted profile picture is a valid profile picture, the user's profile picture is updated. <br/>
        /// If no profile picture is submitted, the user's profile picture is reset to the default profile picture, default.png, found in wwwroot/Images. <br/>
        /// If any errors occur, the appropriate error messages are generated for displaying in the response.</summary>
        /// <returns>The edit information page, with either the success message or an error, if the user is logged in. Otherwise, the home page.</returns>
        [HttpPost]
        public IActionResult editProfilePic() 
        {
            if (!HttpContext.Session.ContainsKey("userInfo")) return RedirectToAction("Index", "Home");
            StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
            IFormCollection form = Request.Form;
            if (form.Files.Count == 0)
            {
                if (FileChecker.Exists(Program.ImageProjectPath + "/" + user.PictureName + ".png"))
                {
                    if (DBConnector.UpdatePictureNameByID(user.Id, ""))
                    {
                        try
                        {
                            FileChecker.Delete(Program.ImageProjectPath + "/" + user.PictureName + ".png");
                            HttpContext.Session.SetString("pictureSuccess", "Profile picture successfully reset!");
                            user.PictureName = "";
                        }
                        catch { HttpContext.Session.SetString("pictureError", "Unable to reset profile picture. Please try again."); }
                    }
                    else HttpContext.Session.SetString("pictureError", "Unable to remove profile picture from DB. Please try again.");
                }
                else HttpContext.Session.SetString("pictureSuccess", "Nothing to reset.");
            }
            else if (form.Files.Count == 1)
            {
                IFormFile file = form.Files[0];
                try
                {
                    if (Path.GetExtension(file.FileName) == ".png" && file.Length < 1000000)
                    {
                        Stream fileStream = file.OpenReadStream();
                        byte[] bytes = new byte[file.Length];
                        fileStream.Read(bytes, 0, (int)file.Length);
                        fileStream.Close();
                        MemoryStream ms = new MemoryStream(bytes);
                        Bitmap bitmap = (Bitmap)Bitmap.FromStream(ms);
                        ms.Close();
                        string fileName = InformationValidator.MD5Hash(InformationValidator.GenerateTemporaryPassword());
                        if (DBConnector.UpdatePictureNameByID(user.Id, fileName))
                        {
                            try
                            {
                                FileChecker.Delete(Program.ImageProjectPath + "/" + user.PictureName + ".png");
                                bitmap.Save(Program.ImageProjectPath + "/" + fileName + ".png", ImageFormat.Png);
                                user.PictureName = fileName;
                                HttpContext.Session.SetString("pictureSuccess", "Profile picture successfully updated!");
                            }
                            catch { HttpContext.Session.SetString("pictureError", "Could not save image to disk. Please try again."); }
                        }
                        else HttpContext.Session.SetString("pictureError", "Could not save file name to DB. Please try again.");
                    }
                    else HttpContext.Session.SetString("pictureError", "To reset your profile picture, submit the form without selecting an image. Otherwise, please ensure your image is a < 1MB .png");
                }
                catch { HttpContext.Session.SetString("pictureError", "Unable to update profile picture. Please Try again."); }
            }
            else HttpContext.Session.SetString("pictureError", "To reset your profile picture, submit the form without selecting an image. Otherwise, please ensure your image is a < 1MB .png");

            HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
            return RedirectToAction("editInfo", "EditInfo");
        }

        /// <summary>Attempts to modify a <see cref="StandardUser"/>'s about based on the submitted form. <br/>
        /// If the user is not logged in, the user is redirected to the home page. <br/>
        /// If the submitted about is a valid about, the user's about is updated in the DB. <br/>
        /// If any errors occur, the appropriate error messages are generated for displaying in the response.</summary>
        /// <returns>The edit information page, with either the success message or an error, if the user is logged in. Otherwise, the home page.</returns>
        [HttpPost]
        public IActionResult editAbout()
        {
            if (!HttpContext.Session.ContainsKey("userInfo")) return RedirectToAction("Index", "Home");
            string newAbout = Request.Form["newAbout"];
            if (newAbout.Length > 255)
            {
                HttpContext.Session.SetString("aboutError", "Please ensure your \"about me\" is less than 256 characters.");
            }
            else
            {
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                if (DBConnector.UpdateAbout(newAbout, user.Id))
                {
                    user.About = newAbout;
                    HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
                    HttpContext.Session.SetString("aboutSuccess", "\"About me\" successfully updated.");
                }
                else
                {
                    HttpContext.Session.SetString("aboutError", "Unable to update your \"about me\". Please try again.");
                }
            }
            return RedirectToAction("editInfo", "EditInfo");
        }
    }
}
