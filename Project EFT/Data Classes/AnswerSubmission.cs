using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class AnswerSubmission : Submission
    {
        public bool IsCorrect { get; set; }

        public int ProblemId { get; set; }

        public AnswerSubmission(string content, DateTime submissionDate, int id, bool isCorrect, int problemID) : base(content, submissionDate, id)
        {
            IsCorrect = isCorrect;
            ProblemId = problemID;
        }

        //for posterity sake, this used to be an override of Equals, which required a custom GetHashCode function as well...it was never committed, for good reason...
        //I will allow you to picture what that monstrosity looked like
        public override bool IsEqual(Submission subs)
        {
            try
            {
                AnswerSubmission Asubs = (AnswerSubmission)subs;
                return (this.Content.Equals(Asubs.Content)
                        && this.SubmissionDate.Equals(Asubs.SubmissionDate)
                        && this.UserID == Asubs.UserID && this.IsCorrect == Asubs.IsCorrect
                        && this.ProblemId == Asubs.ProblemId);
            }catch(Exception e)
            {
                Debug.WriteLine(e);
                Console.WriteLine(e);
            }

            return false;
        }
    }
}
