using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Database;

namespace Project_EFT.Data_Classes
{
    public class StandardUser : User
    {
        public int Ranking { get; set; }
        public Dictionary<int, List<AnswerSubmission>> Submissions { get; set; }
        public int PointsTotal { get; set; }
        public string About { get; set; }


        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id, Dictionary<int, List<AnswerSubmission>> subMap) : base(username, password, email, id)
        {
            Ranking = ranking;
            Submissions = subMap;
            PointsTotal = pointsTotal;
        }


        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id) : base(username, password, email, id)
        {
            PointsTotal = pointsTotal;
            Ranking = ranking;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
        }

        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id, string about) : base(username, password, email, id)
        {
            PointsTotal = pointsTotal;
            Ranking = ranking;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
            About = about;
        }

        public StandardUser(string username, string password, string email) : base(username, password, email) 
        {
            Ranking = 0;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
        }

        public StandardUser() : base()
        { }

        public void UpdateRanking() 
        {
            int newRanking = DBConnector.GetUserRanking(Id);
            if (newRanking != DBConnector.DB_FAILURE) Ranking = newRanking;
        }

        public string GetImagePathString() 
        {
            return InformationValidator.ImageWebPath + "/" + (File.Exists(InformationValidator.ImageProjectPath + "/" + Id + ".png") ? Id.ToString() + ".png" : "default.png");
        }

        public bool IsEqualWithSubMap(StandardUser otherUser)
        {

            //checks the equality of the values in the submission dictionary....
            foreach (KeyValuePair<int, List<AnswerSubmission>> kv in otherUser.Submissions)
            {

                if (this.Submissions.ContainsKey(kv.Key))
                {
                    int count = 0;
                    foreach (AnswerSubmission a in kv.Value)
                    {
                        if (!this.Submissions[kv.Key][count].IsEqual(a))
                        {
                            return false;
                        }
                        count++;
                    }
                }
                else
                {
                    return false;
                }
            }
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && this.Email.Equals(otherUser.Email) && this.Id == otherUser.Id && this.PointsTotal == otherUser.PointsTotal && this.About.Equals(otherUser.About));
        }

        public bool IsEqual(StandardUser otherUser)
        {
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && this.Email.Equals(otherUser.Email) && this.Id == otherUser.Id && this.PointsTotal == otherUser.PointsTotal && this.About.Equals(otherUser.About));
        }

        //the query GetUserProfileInfo does not retrieve the email or password of the user, so as to not store it in the session, so this equal will check based on all other fields returned.
        public bool IsEqualForProfile(StandardUser otherUser)
        {

            //checks the equality of the values in the submission dictionary....
            foreach (KeyValuePair<int, List<AnswerSubmission>> kv in otherUser.Submissions)
            {

                if (this.Submissions.ContainsKey(kv.Key))
                {
                    int count = 0;
                    foreach (AnswerSubmission a in kv.Value)
                    {
                        if (!this.Submissions[kv.Key][count].IsEqual(a))
                        {
                            return false;
                        }
                        count++;
                    }
                }
                else
                {
                    return false;
                }
            }
            return (this.Username.Equals(otherUser.Username) && this.Id == otherUser.Id && this.PointsTotal == otherUser.PointsTotal && this.About.Equals(otherUser.About));
        }

        //when inserting a new user, we dont have their id, about, points, etc.
        public bool IsEqualWithOnlyUsernameEmailPassword(StandardUser otherUser)
        {
            Console.WriteLine(this.Username.Equals(otherUser.Username) + " " + this.Password.Equals(otherUser.Password) + " " + this.Email.Equals(otherUser.Email));
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && this.Email.Equals(otherUser.Email));
        }
    }
}
