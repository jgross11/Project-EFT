using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents an admin user in the system, which is a <see cref="User"/> with an administrative actions <see cref="Submission"/> list.</summary>
    public class Admin : User
    {
        /// <summary>Contains all administrative actions an admin has done, including creating new <see cref="Problem"/>s and deleting <see cref="StandardUser"/>s.</summary>
        public List<Submission> Submissions { get; set; }

        /// <summary>Creates an instance with the given information with an empty <see cref="Submission"/> list.</summary>
        /// <param name="username">The admin's username.</param>
        /// <param name="password">The admin's password.</param>
        /// <param name="email">The admin's email.</param>
        /// <param name="id">The admin's user ID.</param>
        public Admin(string username, string password, string email, int id) : base(username, password, email, id) 
        {
            Submissions = new List<Submission>();
        }

        /// <summary>Creates an empty admin with no <see cref="Submission"/> list.</summary>
        public Admin() : base() { }

        /// <summary>Compares this admin with another, specifically, compares the username, password, email, and ID. Finally, compares every entry in both admin's submissions lists.</summary>
        /// <param name="otherAdmin">The admin to compare to.</param>
        /// <returns>True if all data fields are equal, and this admin's submissions list is equal in size, content, and ordering to the other admin's. False otherwise.</returns>
        public bool IsEqualWithSubList(Admin otherAdmin)
        {
            if (Submissions.Count != otherAdmin.Submissions.Count) return false;
            int count = 0;
            foreach(Submission s in otherAdmin.Submissions)
            {
                if (!this.Submissions[count].IsEqual(s))
                {
                    return false;
                }
                count++;
            }

            return (this.Username.Equals(otherAdmin.Username) && this.Password.Equals(otherAdmin.Password) && this.Email.Equals(otherAdmin.Email) && this.Id == otherAdmin.Id);
        }

        /// <summary>Compares this admin with another, specifically, compares the username, password, email, and ID.</summary>
        /// <param name="otherAdmin">The admin to compare to.</param>
        /// <returns>True if all data fields are equal. False otherwise.</returns>
        public bool IsEqual(Admin otherAdmin)
        {
            return (this.Username.Equals(otherAdmin.Username) && this.Password.Equals(otherAdmin.Password) && this.Email.Equals(otherAdmin.Email) && this.Id == otherAdmin.Id);
        }
    }
}
