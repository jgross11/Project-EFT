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
    public class CipherController : Controller
    {
        private readonly ILogger<CipherController> _logger;

        public CipherController(ILogger<CipherController> logger)
        {
            _logger = logger;
        }

        // encryption POST
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
