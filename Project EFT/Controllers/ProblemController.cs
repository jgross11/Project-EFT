using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
                ViewData["problemNumber"] = problem.ProblemNumber;
                ViewData["problem"] = problem.Question;
                if (HttpContext.Session.ContainsKey("userInfo"))
                {
                    StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
                    ViewData["showProblem"] = !(DBConnector.GetProblemCorrectValueByUserAndProblemID(user.Id, problem.ProblemNumber));
                   
                }
                else
                {
                    ViewData["showProblem"] = false;
                }

            }
            else
            {
                ViewData["ShowPage"] = false;
            }
            // attempts to locate cshtml file with name GenericProblem in GenericProblem folder and Shared folder
            return View();
        }

        //check Answer method POST
        [HttpPost]
        public IActionResult CheckAnswer()
        {

            //retrieves the user from the session
            StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");

            Problem problem = DBConnector.GetProblemByID(int.Parse(Request.Form["problemNumber"]));
           
            //TODO Add this to session?
            //Set the problem information to be passed to the front end
            ViewData["ShowPage"] = true;
            ViewData["Title"] = problem.Title;
            ViewData["problemNumber"] = problem.ProblemNumber;
            ViewData["problem"] = problem.Question;
            ViewData["isCorrect"] = Request.Form["answer"].Equals(problem.Answer);
            ViewData["showProblem"] = !((bool)ViewData["isCorrect"]);
            //creates a new submission and sends it to the DB
            AnswerSubmission answer = new AnswerSubmission(Request.Form["answer"], DateTime.Now, user.Id, (bool)ViewData["isCorrect"], problem.ProblemNumber);
            DBConnector.InsertNewAnswerSubmission(answer);

            return View("Problem");
        }

        public IActionResult AddProblemPage()
        {
            
            return View();
        }


        [HttpPost]
        public IActionResult AddProblem()
        {
            Problem problem = new Problem(0, Request.Form["Title"], Request.Form["question"], Request.Form["answer"], 0, 0);

            if(problem.Title.Equals("") || problem.Question.Equals("") || problem.Answer.Equals(""))
            {
                ViewData["message"] = "One of the entries was left blank, please try again.";
            }
            else
            {
                DBConnector.InsertNewProblem(problem);
                ViewData["message"] = "Your new problem has been added to the list of problems!";
            }



            return View("AddProblemPage");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

