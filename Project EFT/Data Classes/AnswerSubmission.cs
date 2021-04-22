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
        public bool IsEqual(AnswerSubmission subs)
        {
            return (this.Content.Equals(subs.Content)
                    && this.SubmissionDate.Equals(subs.SubmissionDate)
                    && this.UserID == subs.UserID && this.IsCorrect == subs.IsCorrect
                    && this.ProblemId == subs.ProblemId);
        }
    }
}
