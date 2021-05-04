using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents a <see cref="StandardUser"/>'s submission of a <see cref="Problem"/>, including the problem number and the correctness of the submission.</summary>
    public class AnswerSubmission : Submission
    {
        /// <summary>Whether or not this submission's content was correct (exactly equal to the <see cref="Problem"/>'s <see cref="Problem.Answer"/>).</summary>
        public bool IsCorrect { get; set; }

        /// <summary>The number of the <see cref="Problem"/> this submission is for.</summary>
        public int ProblemId { get; set; }

        /// <summary>Creates an instance with the given information representing a complete answer submission.</summary>
        /// <param name="content">This submission's solution.</param>
        /// <param name="submissionDate">The date at which this solution was submitted.</param>
        /// <param name="id">The DB ID of this submission.</param>
        /// <param name="isCorrect">The correctness of this submission.</param>
        /// <param name="problemID">The number of the problem this submission is for.</param>
        public AnswerSubmission(string content, DateTime submissionDate, int id, bool isCorrect, int problemID) : base(content, submissionDate, id)
        {
            IsCorrect = isCorrect;
            ProblemId = problemID;
        }

        /// <summary>Compares this answer submission with another. Specifically, it ensures all field values are equal.</summary>
        /// <param name="subs">The <see cref="AnswerSubmission"/> to compare to.</param>
        /// <returns>True if the other submission is an <see cref="AnswerSubmission"/> and all field values are equal, false otherwise.</returns>
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
