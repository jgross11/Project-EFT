using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_EFT.Models;

namespace Project_EFT.Controllers
{
    public class GenericCipherController : Controller
    {
        private readonly ILogger<GenericCipherController> _logger;

        public GenericCipherController(ILogger<GenericCipherController> logger)
        {
            _logger = logger;
        }

        // localhost.../GenericCipher, see Startup.cs for more info
        public IActionResult GetPage()
        {
            return GenericCipher();
        }

        public IActionResult GenericCipher()
        {
            // must set viewdata here, not in initial GetPage
            ViewData["echo"] = "Hello again, World!";

            // attempts to locate cshtml file with name GenericCipher in GenericCipher folder and Shared folder
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
