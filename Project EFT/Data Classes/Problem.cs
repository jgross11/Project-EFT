using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents a Project-Euler like problem, with a title, question, answer, and number, among other fields.</summary>
    public class Problem
    {
        /// <summary>Positive number that is unique to this problem.</summary>
        public int ProblemNumber { get; set; }

        /// <summary>Title of this problem.</summary>
        public string Title { get; set; }

        /// <summary>Question of this problem.</summary>
        public string Question { get; set; }

        /// <summary>Answer of this problem.</summary>
        public string Answer { get; set; }

        /// <summary>Number of attempts submitted for this problem.</summary>
        public int Attempts { get; set; }

        /// <summary>Number of correct attempts submitted for this problem.</summary>
        public int Completions { get; set; }

        /// <summary>Number of points this value is worth, should a <see cref="StandardUser"/> submit a correct solution.</summary>
        public int PointsValue { get; set; }

        /// <summary>Creates an empty instance with no information.</summary>
        public Problem() { }

        /// <summary>Creates a full instance with the given information.</summary>
        /// <param name="number">The problem number of this problem.</param>
        /// <param name="title">The title of this problem.</param>
        /// <param name="question">The question of this problem.</param>
        /// <param name="answer">The answer of this problem.</param>
        /// <param name="attempts">The number of attempts submitted for this problem.</param>
        /// <param name="completions">The number of correct attempts submitted for this problem.</param>
        /// <param name="pointsValue">The number of points this problem is worth for a correct completion.</param>
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

        /// <summary>Determines if most of the given problem's field values are equal to this one's. <br/>
        /// As attempts and completions for a problem frequently change, and this is primarily used for DB testing,
        /// attempts and completions are not checked for equality, but all other fields are.</summary>
        /// <param name="prob">The problem to compare.</param>
        /// <returns>True if the number, title, question, answer, and point values are equal. False otherwise.</returns>
        public bool IsEqual(Problem prob)
        {
            return (this.ProblemNumber == prob.ProblemNumber
                && this.Title.Equals(prob.Title)
                && this.Question.Equals(prob.Question) && this.Answer.Equals(prob.Answer)
                && this.PointsValue == prob.PointsValue);
        }
    }
}
