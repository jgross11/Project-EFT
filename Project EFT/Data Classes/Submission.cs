using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents a submission that an <see cref="User"/> makes through the front end. <see cref="Admin"/>s create base class instances, 
    /// while <see cref="StandardUser"/>s create <see cref="AnswerSubmission"/>s.</summary>
    public class Submission
    {
        /// <summary>The information contained in a submission.</summary>
        public string Content { get; set; }

        /// <summary>The date and time at which a submission was created.</summary>
        public DateTime SubmissionDate { get; set; }

        /// <summary>The ID of the <see cref="User"/> who created this submission.</summary>
        public int UserID { get; set; }

        /// <summary>Creates a complete submission with the given informaiton.</summary>
        /// <param name="content">This submission's content.</param>
        /// <param name="submissionDate">This date and time this submission was created.</param>
        /// <param name="userID">The ID of the user who created this submission.</param>
        public Submission(string content, DateTime submissionDate, int userID) 
        {
            Content = content;
            SubmissionDate = submissionDate;
            UserID = userID;
        }

        /// <summary>Compares every field value of this submission to another one.</summary>
        /// <param name="subs">The submission to compare to.</param>
        /// <returns>True if all field values of this class are equal. False otherwise.</returns>
        public virtual bool IsEqual(Submission subs)
        {
            return (this.Content.Equals(subs.Content)
                    && this.SubmissionDate.Equals(subs.SubmissionDate)
                    && this.UserID == subs.UserID);
        }

    }
}
