using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Database;

namespace Project_EFT.Data_Classes
{
    /// <summary>Represents a standard user in the system, which is a <see cref="User"/> with a ranking, answer submissions, point total, and profile.</summary>
    public class StandardUser : User
    {
        /// <summary>The ranking of a user in relation to others regarding problem completion.</summary>
        public int Ranking { get; set; }

        /// <summary>The collection of a user's answer submissions, by problem number (key), in a list sorted by completion date (value).</summary>
        public Dictionary<int, List<AnswerSubmission>> Submissions { get; set; }

        /// <summary>The total amount of points a user has obtained by correctly completing problems.</summary>
        public int PointsTotal { get; set; }

        /// <summary>A brief summary of a user that is displayed on their profile.</summary>
        public string About { get; set; }

        /// <summary>Name of this user's profile picture saved in the project.</summary>
        public string PictureName { get; set; }

        /// <summary>Creates an almost complete instance with the given information, which is only missing an about.</summary>
        /// <param name="username">This user's username.</param>
        /// <param name="password">This user's password.</param>
        /// <param name="email">This user's email.</param>
        /// <param name="ranking">This user's ranking.</param>
        /// <param name="pointsTotal">This user's points total.</param>
        /// <param name="id">This user's id.</param>
        /// <param name="subMap">This user's submissions map.</param>
        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id, Dictionary<int, List<AnswerSubmission>> subMap) : base(username, password, email, id)
        {
            Ranking = ranking;
            Submissions = subMap;
            PointsTotal = pointsTotal;
        }

        /// <summary>Creates an instance with the given information with an empty submissions map.</summary>
        /// <param name="username">This user's username.</param>
        /// <param name="password">This user's password.</param>
        /// <param name="email">This user's email.</param>
        /// <param name="ranking">This user's ranking.</param>
        /// <param name="pointsTotal">This user's points total.</param>
        /// <param name="id">This user's id.</param>
        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id) : base(username, password, email, id)
        {
            PointsTotal = pointsTotal;
            Ranking = ranking;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
        }

        /// <summary>Creates an almost complete instance with the given information, which is only missing a non-empty submissions map.</summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="ranking"></param>
        /// <param name="pointsTotal"></param>
        /// <param name="id"></param>
        /// <param name="about"></param>
        public StandardUser(string username, string password, string email, int ranking, int pointsTotal, int id, string about) : base(username, password, email, id)
        {
            PointsTotal = pointsTotal;
            Ranking = ranking;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
            About = about;
        }

        /// <summary>Creates a basic instance with very little information about the user, including an empty submissions map.</summary>
        /// <param name="username">This user's username.</param>
        /// <param name="password">This user's password.</param>
        /// <param name="email">This user's email.</param>
        public StandardUser(string username, string password, string email) : base(username, password, email) 
        {
            Ranking = 0;
            Submissions = new Dictionary<int, List<AnswerSubmission>>();
        }

        /// <summary>Creates an empty instance with no information and a null submissions map.</summary>
        public StandardUser() : base(){ }

        /// <summary>Fetches a user's current ranking from the <see cref="DBConnector"/> and updates this model accordingly.</summary>
        public void UpdateRanking() 
        {
            int newRanking = DBConnector.GetUserRanking(Id);
            if (newRanking != DBConnector.DB_FAILURE) Ranking = newRanking;
        }

        /// <summary>Determines the path to the appropriate picture to display for this user's profile.</summary>
        /// <returns>The image path for an HTML img element to find this user's profile picture if it exists, or the default profile picture otherwise.</returns>
        public string GetImagePathString() 
        {
            return Program.ImageWebPath + "/" + (File.Exists(Program.ImageProjectPath + "/" + PictureName + ".png") ? PictureName + ".png" : "default.png");
        }

        /// <summary>Checks for complete equality with another user, including all fields and submissions.</summary>
        /// <param name="otherUser">The user to compare to.</param>
        /// <returns>True if all field values, including all submissions, are equal. False otherwise.</returns>
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
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && 
                this.Email.Equals(otherUser.Email) && this.Id == otherUser.Id && this.PointsTotal == otherUser.PointsTotal && this.About.Equals(otherUser.About));
        }

        /// <summary>Checks for equality for all fields with another user, except submissions.</summary>
        /// <param name="otherUser">The user to compare to.</param>
        /// <returns>True if all field values are equal. False otherwise.</returns>
        public bool IsEqual(StandardUser otherUser)
        {
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && this.Email.Equals(otherUser.Email) && this.Id == otherUser.Id && this.PointsTotal == otherUser.PointsTotal && this.About.Equals(otherUser.About));
        }

        /// <summary>Primarily used for DB testing, compares this user's profile information.</summary>
        /// <param name="otherUser">The user to compare to.</param>
        /// <returns>True if all submissions, username, id, points total, and about are equal. False otherwise.</returns>
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

        /// <summary>Primarily used for DB testing, compares this user's login / account information.</summary>
        /// <param name="otherUser">The user to compare to.</param>
        /// <returns>True if the username, password, and email are equal. False otherwise.</returns>
        public bool IsEqualWithOnlyUsernameEmailPassword(StandardUser otherUser)
        {
            Console.WriteLine(this.Username.Equals(otherUser.Username) + " " + this.Password.Equals(otherUser.Password) + " " + this.Email.Equals(otherUser.Email));
            return (this.Username.Equals(otherUser.Username) && this.Password.Equals(otherUser.Password) && this.Email.Equals(otherUser.Email));
        }
    }
}
