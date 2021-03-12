using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_EFT.Data_Classes;
using Project_EFT.Models;

namespace Project_EFT.Controllers
{
    public class CipherController : Controller
    {
        private readonly ILogger<CipherController> _logger;

        public CipherController(ILogger<CipherController> logger)
        {
            _logger = logger;
        }

        // localhost.../Cipher?name=(name), see Startup.cs for more info
        public IActionResult GetPage(String name)
        {
            return Cipher(name);
        }

        // encryption POST
        [HttpPost]
        public IActionResult encrypt()
        {
            // debug form contents
            Debug.WriteLine("From request");
            Debug.WriteLine(Request.Form["alphabet"]);
            Debug.WriteLine(Request.Form["encryptShiftAmount"]);
            Debug.WriteLine(Request.Form["plainTextInput"]);

            // create instance of cipher, do encryption
            string ciphertext = new CaesarCipher(Request.Form["alphabet"], 0, int.Parse(Request.Form["encryptShiftAmount"]), 1).Encrypt(Request.Form["plainTextInput"]);
            
            // store ciphertext in view map
            ViewData["cipherTextInput"] = ciphertext;

            // store other form contents to populate form 
            ViewData["alphabet"] = Request.Form["alphabet"];
            ViewData["decryptShiftAmount"] = Request.Form["decryptShiftAmount"];
            ViewData["encryptShiftAmount"] = Request.Form["encryptShiftAmount"];
            ViewData["plainTextInput"] = Request.Form["plainTextInput"];

            // refresh page
            return View("Cipher");
        }

        [HttpPost]
        public IActionResult decrypt() 
        {

            // debug form contents
            Debug.WriteLine("From request");
            Debug.WriteLine(Request.Form["alphabet"]);
            Debug.WriteLine(Request.Form["decryptShiftAmount"]);
            Debug.WriteLine(Request.Form["cipherTextInput"]);

            // do decryption
            string plaintext = new CaesarCipher(Request.Form["alphabet"], int.Parse(Request.Form["decryptShiftAmount"]), 0, 1).Decrypt(Request.Form["cipherTextInput"])[0];

            // store plain text in view map
            ViewData["plainTextInput"] = plaintext;

            // store other form contents to populate form 
            ViewData["alphabet"] = Request.Form["alphabet"];
            ViewData["encryptShiftAmount"] = Request.Form["encryptShiftAmount"];
            ViewData["decryptShiftAmount"] = Request.Form["decryptShiftAmount"];
            ViewData["cipherTextInput"] = Request.Form["cipherTextInput"];

            return View("Cipher");
        }

        //TODO: Have this change more than just the Title of the page, currently regardless of name, you are sent to 
        //the caesar cipher page...
        public IActionResult Cipher(string name)
        {
            // must set viewdata here, not in initial GetPage
            ViewData["name"] = name;

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
