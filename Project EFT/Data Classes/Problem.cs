using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    // data class representing a Project Euler-type Problem
    public class Problem
    {
        public int problemNumber { get; set; }
        public string title { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
        public int attempts { get; set; }
        public int completions { get; set; }

        // null constructor for convenience / piecewise field setting
        public Problem() 
        { 
        
        }

        public Problem(int number, string title, string question, string answer, int attempts, int completions) 
        {
            this.problemNumber = number;
            this.title = title;
            this.question = question;
            this.answer = answer;
            this.attempts = attempts;
            this.completions = completions;
        }
    }
}
