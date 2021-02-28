using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Project_EFT.Models;
using Project_EFT.Database;
using Project_EFT.Data_Classes;

namespace Project_EFT.Controllers
{
    public class ProblemController : Controller
    {
        private readonly ILogger<ProblemController> _logger;

        public ProblemController(ILogger<ProblemController> logger)
        {
            _logger = logger;
        }

        // localhost.../GenericProblem, see Startup.cs for more info
        public IActionResult GetPage()
        {
            return Problem();
        }

        public IActionResult Problem(int ID)
        {
            //requests a specific problem from the DB, by its ID
            //TODO have the ID be generic based on the number passed by the URL
            Problem problem = DBConnector.GetProblemByID(ID);

            //checks to see if the problem returned from the DB is an actual problem or not
            if (problem.ProblemNumber != 0)
            {
                //Set the problem information to be passed to the front end
                ViewData["ShowPage"] = true;
                ViewData["Title"] = problem.Title;
                ViewData["problemNumber"] = "This is problem number " + problem.ProblemNumber + ".";
                ViewData["problem"] = problem.Question;
            }
            else
            {
                ViewData["ShowPage"] = false;
            }
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

