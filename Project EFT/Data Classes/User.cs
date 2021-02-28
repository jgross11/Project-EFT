using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public abstract class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Id { get; set; }

        public User(string username, string password, string email, int id) 
        {
            Username = username;
            Password = password;
            Email = email;
            Id = id;
        }

        public User(string username, string password, string email) 
        {
            Username = username;
            Password = password;
            Email = email;
            Id = 0;
        }

        public User() 
        { 
        
        }
    }
}
