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
            
            // check if id is valid before fetching from problem list
            if (ID > 0 && ID <= DBConnector.problems.Count)
            {
                // problems cached
                Problem problem = DBConnector.problems[ID - 1];

                //add the problem to the session
                HttpContext.Session.SetComplexObject("problem", problem);

            }
            else
            {
                HttpContext.Session.Remove("problem");
            }

            // attempts to locate cshtml file with name Problem in Problem folder and Shared folder
            return View();
        }

        //check Answer method POST
        [HttpPost]
        public IActionResult CheckAnswer()
        {
            if (!HttpContext.Session.ContainsKey("userInfo")) return RedirectToAction("Index", "Home");
            if (!HttpContext.Session.ContainsKey("problem")) return RedirectToAction("ProblemList", "Home");

            if (!InformationValidator.VerifyInformation(Request.Form["answer"], InformationValidator.ProblemSubmissionType))
            {
                HttpContext.Session.SetString("errorMessage", InformationValidator.InvalidProblemSubmissionString);
            }
            else
            {
                //retrieves the user from the session
                StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");

                //retrieves problem from the session
                Problem problem = HttpContext.Session.GetComplexObject<Problem>("problem");

                if (!DBConnector.CheckIfUserSubmissionTableExists(user.Id))
                {
                    if (DBConnector.CreateUserSubmissionTable(user.Id))
                    {
                        //Set the correctness information to be passed to the front end
                        ViewData["isCorrect"] = ((string)Request.Form["answer"]).Trim().ToLower().Equals(problem.Answer);


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
                    }
                    else
                    {
                        HttpContext.Session.SetString("errorMessage", "Something went wrong, please try submitting your answer again.");
                    }
                }
                else
                {
                    if (user.Submissions.ContainsKey(problem.ProblemNumber))
                    {
                        List<AnswerSubmission> subs = user.Submissions[problem.ProblemNumber];
                        if (subs[subs.Count - 1].IsCorrect)
                        {
                            return View("Problem");
                        }
                    }

                    //Set the correctness information to be passed to the front end
                    ViewData["isCorrect"] = ((string)Request.Form["answer"]).Trim().ToLower().Equals(problem.Answer);


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
                }
            }

            return View("Problem");
        }

        public IActionResult AddProblemPage()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");
            return View();
        }


        [HttpPost]
        public IActionResult AddProblem()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");
            string title = Request.Form["Title"];
            string question = Request.Form["question"];
            string answer = ((string)Request.Form["answer"]).Trim().ToLower();
            bool formattingErrorExists = false;

            if (!InformationValidator.VerifyInformation(title, InformationValidator.ProblemTitleType)) 
            {
                HttpContext.Session.SetString("titleErrorMessage", InformationValidator.InvalidProblemTitleString);
                formattingErrorExists = true;
            }
            if (!InformationValidator.VerifyInformation(question, InformationValidator.ProblemSubmissionType))
            {
                HttpContext.Session.SetString("questionErrorMessage", InformationValidator.InvalidProblemSubmissionString);
                formattingErrorExists = true;
            }
            if (!InformationValidator.VerifyInformation(answer, InformationValidator.ProblemSubmissionType))
            {
                HttpContext.Session.SetString("answerErrorMessage", InformationValidator.InvalidProblemSubmissionString);
                formattingErrorExists = true;
            }
            if (!formattingErrorExists)
            {
                Problem problem = new Problem(0, title, question, answer, 0, 0);
                if (DBConnector.InsertNewProblem(problem))
                {
                    Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                    Problem newProblem = DBConnector.problems[DBConnector.problems.Count - 1];
                    Submission sub = new Submission("Created problem #" + newProblem.ProblemNumber + ": " + newProblem.Title, DateTime.Now, admin.Id);
                    if (DBConnector.InsertNewAdminSubmission(sub))
                    {
                        admin.Submissions.Add(sub);
                        HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                    }
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

