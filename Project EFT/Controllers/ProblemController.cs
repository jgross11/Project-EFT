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
    /// <summary>Handles GETs and POSTs for: <br/>
    /// - Accessing a problem's information (GET). <br/>
    /// - Resetting a user's submissions for a specific problem (GET). <br/>
    /// - Determining the validity of a user's submission to a problem (POST). <br/> 
    /// - Accessing the new problem form (GET). <br/>
    /// - Adding a new problem to the system as an admin (POST). <br/>
    /// </summary>
    public class ProblemController : Controller
    {
        private readonly ILogger<ProblemController> _logger;

        public ProblemController(ILogger<ProblemController> logger)
        {
            _logger = logger;
        }

        /// <summary>Front facing retrieval of a <see cref="Data_Classes.Problem"/>'s information by it's problem number.</summary>
        /// <param name="ID">The number of the problem whose information is desired.</param>
        /// <returns>The Problem page, with the desired problem's information if given a valid number; an invalid problem error page otherwise.</returns>
        public IActionResult GetPage(int ID)
        {
            return Problem(ID);
        }

        /// <summary>Workhorse retrieval of a <see cref="Data_Classes.Problem"/>'s information by it's problem number. <br/>
        /// If the given problem number if valid, the problem will be stored in the session. <br/>
        /// Otherwise, an invalid problem number error message is displayed.</summary>
        /// <param name="ID">The number of the problem whose information is desired.</param>
        /// <returns>The Problem page, with either the desired problem's information if given a valid number, or an invalid problem error page otherwise.</returns>
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

        /// <summary>Attempts to wipe a <see cref="StandardUser"/>'s submission for the given <see cref="Data_Classes.Problem"/>.<br/>
        /// If the problem ID is invalid, or the user is not logged in as a standard user, the user is redirected to the previous page. <br/>
        /// If the user does not have any submissions for the given problem, the user is redirected to the Problem List page. <br/>
        /// Otherwise, the user's submissions for the given problem are deleted from the DB.</summary>
        /// <param name="problemID">The number of the problem whose submissions will be deleted.</param>
        /// <returns>The Problem List page, if the user is logged in and submits a valid problem number they do not have submissions for. The previous page, otherwise.</returns>
        public IActionResult WipeProblemSubmissions(int problemID) 
        {
            if (!HttpContext.Session.ContainsKey("userInfo") || problemID < 1 || problemID > DBConnector.problems.Count) return Redirect(Request.Headers["Referer"].ToString() != "" ? Request.Headers["Referer"].ToString() : "/");
            StandardUser user = HttpContext.Session.GetComplexObject<StandardUser>("userInfo");
            if (!user.Submissions.ContainsKey(problemID)) return RedirectToAction("ProblemList", "Home");
            if (!DBConnector.ResetProblemSubmissions(user, problemID))
                HttpContext.Session.SetString("problem" + problemID + "error", "Unable to wipe submission information for the selected problem. Please try again.");
            else
            {
                user.PointsTotal -= DBConnector.problems[problemID - 1].PointsValue;
                user.UpdateRanking();
                user.Submissions.Remove(problemID);
                HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
            }
            return Redirect(Request.Headers["Referer"].ToString() != "" ? Request.Headers["Referer"].ToString() : "/");
        }

        /// <summary>Nasty workaround for 405 error that occurs after resetting problem submissions directly after submitting a solution.</summary>
        /// <param name="i">The problem number of the current problem - not used at all.</param>
        /// <returns>The Problem List page, if no problem is in the session. Otherwise, the session problem's information on the Problem page.</returns>
        public IActionResult CheckAnswer(int i)
        {
            if (!HttpContext.Session.ContainsKey("problem")) return RedirectToAction("ProblemList", "Home");
            
            return View("Problem", HttpContext.Session.GetComplexObject<Problem>("problem").ProblemNumber);
        }

        /// <summary>Attempts to verify a <see cref="StandardUser"/>'s submitted solution to a <see cref="Data_Classes.Problem"/>. <br/>
        /// Redirects to the home page if the user is not logged in as a standard user. <br/>
        /// Redirects to the Problem List page if the user does not have a problem in the session. <br/>
        /// If the submission is invalid, an error message is generated. <br/>
        /// Otherwise, the submission is sent to the DB, handling both the case where the user has no previous submissions and where previous submissions exist. <br/>
        /// If any DB errors occur, the appropriate error messages are generated and stored for displaying in the response.</summary>
        /// <returns>A page, depending on the logic described above.</returns>
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
                    if (!DBConnector.CreateUserSubmissionTable(user.Id))
                    {
                        HttpContext.Session.SetString("errorMessage", "Something went wrong, please try submitting your answer again.");
                        return View("Problem");
                    }
                }
                else if (user.Submissions.ContainsKey(problem.ProblemNumber))
                {
                    List<AnswerSubmission> subs = user.Submissions[problem.ProblemNumber];
                    if (subs[^1].IsCorrect)
                    {
                        return View("Problem");
                    }
                }

                //Set the correctness information to be passed to the front end
                ViewData["isCorrect"] = ((string)Request.Form["answer"]).Trim().ToLower().Equals(problem.Answer);


                //creates a new submission and sends it to the DB
                AnswerSubmission answer = new AnswerSubmission(Request.Form["answer"], DateTime.Now, user.Id, (bool)ViewData["isCorrect"], problem.ProblemNumber);
                if (DBConnector.InsertNewAnswerSubmission(answer, user.PointsTotal, problem.PointsValue))
                {
                    //add submission to the current user in the session's map and reset the user in the session
                    if (user.Submissions.ContainsKey(answer.ProblemId))
                    {
                        user.Submissions[answer.ProblemId].Add(answer);
                    }
                    else
                    {
                        List<AnswerSubmission> newSubList = new List<AnswerSubmission> { answer };
                        user.Submissions.Add(answer.ProblemId, newSubList);
                    }

                    if ((bool)ViewData["isCorrect"]) 
                    {
                        user.PointsTotal += problem.PointsValue;
                        user.UpdateRanking();
                    }
                    HttpContext.Session.SetComplexObject<StandardUser>("userInfo", user);
                }
                else
                {
                    HttpContext.Session.SetString("errorMessage", "Something went wrong, please try submitting your answer again.");
                }
            }
            return View("Problem");
        }

        /// <summary>Determines if the Add Problem page should be retrieved and acts accordingly.</summary>
        /// <returns>The Add Problem page, if the user is logged in as an <see cref="Admin"/>. The home page, otherwise.</returns>
        public IActionResult AddProblemPage()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");
            return View();
        }

        /// <summary>Attempts to add a new <see cref="Data_Classes.Problem"/> to the system with the given information.<br/>
        /// If the user is not logged in as an <see cref="Admin"/>, redirects to the home page. <br/>
        /// All aspects of the problem are verified, and if any validation errors occur, appropriate error messages are generated for rendering in the response. <br/>
        /// If no validation errors occur, the problem is added to the DB - DB errors are treated exactly as validation errors.</summary>
        /// <returns>The home page, if the user is not logged in as an admin. Otherwise, the Add Problem page, with either error or success message(s).</returns>
        [HttpPost]
        public IActionResult AddProblem()
        {
            if (HttpContext.Session.ContainsKey("userInfo") || !HttpContext.Session.ContainsKey("adminInfo")) return RedirectToAction("Index", "Home");
            string title = Request.Form["Title"];
            string question = Request.Form["question"];
            string answer = ((string)Request.Form["answer"]).Trim().ToLower();
            string value = (string)Request.Form["value"];
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
            if (!InformationValidator.VerifyInformation(value, InformationValidator.ProblemValueType)) 
            {
                HttpContext.Session.SetString("valueErrorMessage", InformationValidator.InvalidProblemValueString);
                formattingErrorExists = true;
            }
            if (!formattingErrorExists)
            {
                Problem problem = new Problem(0, title, question, answer, 0, 0, int.Parse(value));
                if (DBConnector.InsertNewProblem(problem))
                {
                    Admin admin = HttpContext.Session.GetComplexObject<Admin>("adminInfo");
                    Problem newProblem = DBConnector.problems[^1];
                    Submission sub = new Submission("Created problem #" + newProblem.ProblemNumber + ": " + newProblem.Title, DateTime.Now, admin.Id);
                    if (DBConnector.InsertNewAdminSubmission(sub))
                    {
                        admin.Submissions.Add(sub);
                        HttpContext.Session.SetComplexObject<Admin>("adminInfo", admin);
                        ViewData["message"] = "Your new problem has been added to the list of problems!";
                    }
                    else 
                    {
                        ViewData["message"] = "Your problem was added but you were not credited as the author due to an error.";
                    }
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

