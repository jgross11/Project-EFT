using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
