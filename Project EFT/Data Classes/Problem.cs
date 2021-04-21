using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    // data class representing a Project Euler-type Problem
    public class Problem
    {
        public int ProblemNumber { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Attempts { get; set; }
        public int Completions { get; set; }
        public int PointsValue { get; set; }

        // null constructor for convenience / piecewise field setting
        public Problem() 
        { 
        
        }

        public Problem(int number, string title, string question, string answer, int attempts, int completions, int pointsValue) 
        {
            ProblemNumber = number;
            Title = title;
            Question = question;
            Answer = answer;
            Attempts = attempts;
            Completions = completions;
            PointsValue = pointsValue;
        }
    }
}
