using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents a user of this system, and includes everything a typical user does: username, email, password, id.<br/>
    /// Actual user classes are <see cref="Admin"/>, which are admins of the site, and <see cref="StandardUser"/>, which are 'customer' users.</summary>
    public abstract class User
    {
        /// <summary>A user's username.</summary>
        public string Username { get; set; }

        /// <summary>A user's password.</summary>
        public string Password { get; set; }

        /// <summary>A user's email.</summary>
        public string Email { get; set; }

        /// <summary>A user's ID.</summary>
        public int Id { get; set; }

        /// <summary>Completely sets the basic information for a user with the given information.</summary>
        /// <param name="username">This user's username.</param>
        /// <param name="password">This user's password.</param>
        /// <param name="email">This user's email.</param>
        /// <param name="id">This user's ID.</param>
        public User(string username, string password, string email, int id) 
        {
            Username = username;
            Password = password;
            Email = email;
            Id = id;
        }

        /// <summary>Sets the basic information, excluding the ID, for a user with the given information.</summary>
        /// <param name="username">This user's username.</param>
        /// <param name="password">This user's password.</param>
        /// <param name="email">This user's email.</param>
        public User(string username, string password, string email) 
        {
            Username = username;
            Password = password;
            Email = email;
            Id = 0;
        }

        /// <summary>Does absolutely nothing. Probably shouldn't exist.</summary>
        public User() { }
    }
}
