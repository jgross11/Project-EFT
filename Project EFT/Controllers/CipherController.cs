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

namespace Project_EFT.Controllers
{
    /// <summary>Handles GETs and POSTs for: <br/>
    /// - Cipher system encrypting and decrypting (POST).<br/>
    /// - Targetting a specific cipher system for encrypting / decrypting purposes (GET).</summary>
    public class CipherController : Controller
    {
        private readonly ILogger<CipherController> _logger;

        public CipherController(ILogger<CipherController> logger)
        {
            _logger = logger;
        }

        
        /// <summary>Attempts to execute the selected cipher system's encrypt method, based on the submitted encryption form data.<br/>
        /// If no system is selected, the user is redirected to the CipherList page. <br/>
        /// If encryption errors occur, generates error messages to be displayed in front end response.<br/>
        /// If a system is selected, regardless of whether encryption is successfully or not, the user is then redirected to the Cipher page to display results / errors.
        /// </summary>
        /// <returns>The generic Cipher page, formatted with the encryption results if successful, or the appropriate error message(s) otherwise.</returns>
        [HttpPost]
        public IActionResult encrypt()
        {
            Cipher activeSystem = HttpContext.Session.GetComplexObject<Cipher>("activeSystem");

            if (activeSystem != null)
            {
                foreach (Option opt in activeSystem.EncryptionFormOptions)
                {
                    opt.ObtainValueFromForm(Request.Form);
                }

                activeSystem.ResetErrors();

                activeSystem.VerifyAndEncrypt();

                HttpContext.Session.SetComplexObject<Cipher>("activeSystem", activeSystem);

                // refresh page
                return View("Cipher");
            }
            else 
            { 
                return RedirectToAction("CipherList", "Home"); 
            }
        }

        /// <summary>Attempts to execute the selected cipher system's decrypt method, based on the submitted decryption form data.<br/>
        /// If no system is selected, the user is redirected to the CipherList page. <br/>
        /// If decryption errors occur, generates error messages to be displayed in front end response.<br/>
        /// If a system is selected, regardless of whether decryption is successfully or not, the user is then redirected to the Cipher page to display results / errors.
        /// </summary>
        /// <returns>The generic Cipher page, formatted with the decryption results if successful, or the appropriate error message(s) otherwise.</returns>
        [HttpPost]
        public IActionResult decrypt() 
        {

            Cipher activeSystem = HttpContext.Session.GetComplexObject<Cipher>("activeSystem");
            if (activeSystem != null)
            {
                foreach (Option opt in activeSystem.DecryptionFormOptions)
                {
                    opt.ObtainValueFromForm(Request.Form);
                }

                activeSystem.ResetErrors();

                activeSystem.VerifyAndDecrypt();

                HttpContext.Session.SetComplexObject<Cipher>("activeSystem", activeSystem);

                // refresh page
                return View("Cipher");
            }
            else
            {
                return RedirectToAction("CipherList", "Home");
            }
        }

        /// <summary>Attempts to create an instance of the selected <see cref="Cipher"/> by it's index in <see cref="Program.CipherList"/> for encrypting or decrypting purposes.<br/>
        /// If no cipher exists at the given index, the response will indicate as such. Otherwise, the selected cipher's encrypt / decrypt form are displayed in the response.</summary>
        /// <param name="id">The index in <see cref="Program.CipherList"/> of the desired <see cref="Cipher"/> system.</param>
        /// <returns>The cipher page, that displays a cipher's form information, or an error message indicating no such cipher system exists.</returns>
        public IActionResult cipher(int id)
        {
            if (id > -1 && id < Program.CipherList.Count)
            {
                HttpContext.Session.SetComplexObject<Cipher>("activeSystem", (Cipher)Activator.CreateInstance(Program.CipherList[id]));
            }
            else 
            {
                HttpContext.Session.Remove("activeSystem");
            }

            // attempts to locate cshtml file with name Cipher in Cipher folder and Shared folder
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
