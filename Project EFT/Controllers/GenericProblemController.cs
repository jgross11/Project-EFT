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
    public class GenericProblemController : Controller
    {
        private readonly ILogger<GenericProblemController> _logger;

        public GenericProblemController(ILogger<GenericProblemController> logger)
        {
            _logger = logger;
        }

        // localhost.../GenericProblem, see Startup.cs for more info
        public IActionResult GetPage()
        {
            return GenericProblem();
        }

        public IActionResult GenericProblem()
        {

            Random rand = new Random();
            int number = rand.Next(1, 1000);
            ViewData["problemNumber"] = "This is problem number " + number + ".";
            ViewData["problem"] = "wklv lv d whvw";
            // attempts to locate cshtml file with name GenericProblem in GenericProblem folder and Shared folder
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

