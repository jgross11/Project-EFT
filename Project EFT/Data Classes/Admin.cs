using System;
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

        public bool IsEqualWithSubList(Admin otherAdmin)
        {
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

        public bool IsEqual(Admin otherAdmin)
        {
            return (this.Username.Equals(otherAdmin.Username) && this.Password.Equals(otherAdmin.Password) && this.Email.Equals(otherAdmin.Email) && this.Id == otherAdmin.Id);
        }
    }
}
