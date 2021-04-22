using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        //the attempts/completions of a problem is variable, so tests involving them would fail
        //this is really testing whether or not the query is going to return the values in the first place
        //and I see no other use case for equating problems in this way, or any way
        public bool IsEqual(Problem prob)
        {
            return (this.ProblemNumber == prob.ProblemNumber
                && this.Title.Equals(prob.Title)
                && this.Question.Equals(prob.Question) && this.Answer.Equals(prob.Answer)
                && this.PointsValue == prob.PointsValue);
        }
    }
}
