using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class AnswerSubmission : Submission
    {
        public bool IsCorrect { get; set; }

        public AnswerSubmission(string content, DateTime submissionDate, bool isCorrect) : base(content, submissionDate)
        {
            IsCorrect = isCorrect;
        }
    }
}
