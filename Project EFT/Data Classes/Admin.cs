﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public class Admin : User
    {
        public List<Submission> Submissions { get; set; }

        public Admin(string username, string password, string email, int id) : base(username, password, email, id) 
        {
            Submissions = new List<Submission>();
        }

        public Admin() : base() 
        { 
        
        }

        public string Encode()
        {
            return "";
        }

        public static Admin Decode()
        {
            return new Admin();
        }
    }
}
