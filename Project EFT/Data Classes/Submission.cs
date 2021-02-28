using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class Submission
    {
        public string Content { get; set; }
        public DateTime SubmissionDate { get; set; }

        public Submission(string content, DateTime submissionDate) 
        {
            Content = content;
            SubmissionDate = submissionDate;
        }
    }
}
