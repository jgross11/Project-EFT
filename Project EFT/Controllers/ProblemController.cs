﻿using System;
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
        public IActionResult GetPage(int ID)
        {
            return Problem(ID);
        }

        public IActionResult Problem(int ID)
        {
            //requests a specific problem from the DB, by its ID
            Problem problem = DBConnector.GetProblemByID(ID);
            
            //checks to see if the problem returned from the DB is an actual problem or not
            if (problem.ProblemNumber != 0)
            {

                //add the problem to the session
                HttpContext.Session.SetComplexObject("problem", problem);
                
            }
           
            // attempts to locate cshtml file with name Problem in Problem folder and Shared folder
            return View();
        }

        //check Answer method POST
        [HttpPost]
        public IActionResult CheckAnswer()
        {

            //retrieves the user from the session
            StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");

            //retrieves problem from the session
            Problem problem = HttpContext.Session.GetComplexObject<Problem>("problem");

            
            //TODO Find a nice way to get rid of this, I could just be dumb, but I cant find a temporary way to see if a user JUST got it correct, which is what this controls
            //on the front end, that is, saying Correct/Incorrect

            //Set the correctness information to be passed to the front end
            ViewData["isCorrect"] = Request.Form["answer"].Equals(problem.Answer);
            

            //creates a new submission and sends it to the DB
            AnswerSubmission answer = new AnswerSubmission(Request.Form["answer"], DateTime.Now, user.Id, (bool)ViewData["isCorrect"], problem.ProblemNumber);
            DBConnector.InsertNewAnswerSubmission(answer);

            //add submission to the current user in the session's map and reset the user in the session
            if (user.Submissions.ContainsKey(answer.ProblemId))
            {
                user.Submissions[answer.ProblemId].Add(answer);
            }
            else
            {
                List<AnswerSubmission> newSubList = new List<AnswerSubmission>();
                newSubList.Add(answer);
                user.Submissions.Add(answer.ProblemId, newSubList);
            }
            HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);

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
                if (DBConnector.InsertNewProblem(problem))
                {
                    ViewData["message"] = "Your new problem has been added to the list of problems!";
                }
                else
                {
                    ViewData["message"] = "Something went wrong and your problem wasn't added, please try again.";
                }
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

