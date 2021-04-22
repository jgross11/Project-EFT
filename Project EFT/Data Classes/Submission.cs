using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class Submission
    {
        public string Content { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int UserID { get; set; }

        public Submission(string content, DateTime submissionDate, int userID) 
        {
            Content = content;
            SubmissionDate = submissionDate;
            UserID = userID;
        }

        //for posterity sake, this used to be an override of Equals, which required a custom GetHashCode function as well...it was never committed, for good reason...
        //I will allow you to picture what that monstrosity looked like
        public bool IsEqual(Submission subs)
        {
            return (this.Content.Equals(subs.Content)
                    && this.SubmissionDate.Equals(subs.SubmissionDate)
                    && this.UserID == subs.UserID);
        }

    }
}
