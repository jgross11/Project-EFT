using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_EFT.Data_Classes;
using Project_EFT.Database;
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

            // fetch problems from DB and render in view
            ViewData["problems"] = DBConnector.GetProblemsList();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
