﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class StandardUser : User
    {
        public int Ranking { get; set; }
        public Dictionary<int, AnswerSubmission> Submissions { get; set; }

        public StandardUser(string username, string password, string email, int ranking, int id) : base(username, password, email, id)
        {
            Ranking = ranking;
            Submissions = new Dictionary<int, AnswerSubmission>();
        }

        public StandardUser(string username, string password, string email) : base(username, password, email) 
        {
            Ranking = 0;
            Submissions = new Dictionary<int, AnswerSubmission>();
        }

        public StandardUser() : base()
        { }
    }
}
