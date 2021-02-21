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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // attempts to locate cshtml file with name Index in Home folder and Shared folder
        public IActionResult Index()
        {
            return View();
        }

        // localhost.../Home/GenericCipher
        public IActionResult GenericCipher()
        {
            ViewData["echo"] = "Hello, World!";

            // attempts to locate cshtml file with name GenericCipher in Home folder and Shared folder
            return View();
        }

        public IActionResult Privacy()
        {
            // attempts to locate cshtml file with name Privacy in Home folder and Shared folder
            return View();
        }

        public IActionResult GenericList() 
        {

            // create a sample list of problems to display on the webpage
            Problem[] problems = new Problem[3];
            problems[0] = new Problem(1, "A Simple Caesar", "Decrypt the following Caesar cipher: wkdw", "that", 100, 85);
            problems[1] = new Problem(2, "A Less Simple Caesar", "Decrypt the following Caesar cipher: wkdw d", "that a", 50, 27);
            problems[2] = new Problem(3, "A Much Less Simple Caesar", "Decrypt the following Caesar cipher: wkdw dw", "that at", 10, 2);

            // store problem array in view data
            ViewData["problems"] = problems;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
