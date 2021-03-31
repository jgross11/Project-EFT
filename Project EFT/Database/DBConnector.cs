using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Project_EFT.Data_Classes;
using System.Diagnostics;

namespace Project_EFT.Database
{
    public class DBConnector
    {
        public static MySqlConnection connection;
        public static string connectionString;
        public static List<Problem> problems;

        public const int DB_FAILURE = -1;
        public const int CREDENTIAL_CHANGE_SUCCESS = 100;
        public const int CREDENTIAL_TAKEN = 101;

        public static void Init()
        {
            string[] lines = System.IO.File.ReadAllLines("../../info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];

            // cache list of all problems currently in db
            GetProblemsList();
        }

        public static bool OpenConnection()
        {
            // dependent on the structure of the file...
            connection = new MySqlConnection(connectionString);
            connection.Open();
            return true;
        }

        public static MySqlCommand MakeCommand(string statement)
        {
            OpenConnection();
            return new MySqlCommand(statement, connection);
        }

        //this query is just gross, and I considered deleting it...however, it works(update: I should say "worked", now that the subqueries have been changed, this really should be deleted...)
        //...and should at least be in here for documentation as to
        //how I converted the AnswerSubmissions table into UserSubmissions for specific users without deleting their data...this is how
        /*public static void CreateNewTablesAndPopulateOldData()
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users");
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int UserId = reader.GetInt32(0);
                CreateUserSubmissionTable(UserId);

                List<AnswerSubmission> SubList = GetAnswerSubmissionsByID(UserId);
                string tablename = "UserSubmissions" + UserId;
                foreach(AnswerSubmission submission in SubList)
                {
                    command = MakeCommand("INSERT INTO " + tablename + "(UserSubmissions_Answer, UserSubmissions_SubmissionDate, UserSubmissions_IsCorrect, USerSubmissions_ProblemID) VALUES(@content, @date, @correct, @problem_id)");
                    command.Parameters.AddWithValue("@content", submission.Content);
                    command.Parameters.AddWithValue("@date", submission.SubmissionDate);
                    command.Parameters.AddWithValue("@correct", submission.IsCorrect);
                    command.Parameters.AddWithValue("@problem_id", submission.ProblemId);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }

        }*/
        public static void CreateUserSubmissionTable(int ID)
        {
            string tablename = "UserSubmissions" + ID;
            MySqlCommand command = MakeCommand("CREATE TABLE " + tablename + " (UserSubmissions_ID INT NOT NULL AUTO_INCREMENT, UserSubmissions_Answer VARCHAR(255) NOT NULL, UserSubmissions_SubmissionDate DATETIME NULL, UserSubmissions_IsCorrect TINYINT NOT NULL, UserSubmissions_ProblemID INT NOT NULL, PRIMARY KEY(UserSubmissions_ID))");
            command.Prepare();
            command.ExecuteNonQuery();
            connection.Close();

        }
        public static bool InsertNewProblem(Problem problem)
        {
            MySqlCommand command = MakeCommand("INSERT INTO Problems (Problem_Title, Problem_Question, Problem_Answer) VALUES (@title, @question, @answer)");
            command.Parameters.AddWithValue("@title", problem.Title);
            command.Parameters.AddWithValue("@question", problem.Question);
            command.Parameters.AddWithValue("@answer", problem.Answer);
            command.Prepare();
            int numRowsAffected = command.ExecuteNonQuery();
            connection.Close();

            if (numRowsAffected == 1)
            {
                problem.ProblemNumber = (int)command.LastInsertedId;
                problems.Add(problem);
                return true;
            }
            return false;
        }

        public static int TryUpdateUsername(User user, string newUsername)
        {
            string firstCommand;
            string secondCommand;
            if (user.GetType() == typeof(Admin))
            {
                firstCommand = "SELECT Admin_ID FROM Admins WHERE Admin_Username = @username";
                secondCommand = "UPDATE Admins SET Admin_Username = @username WHERE Admin_ID = @id";
            }
            else if (user.GetType() == typeof(StandardUser))
            {
                firstCommand = "SELECT User_ID FROM Users WHERE User_Username = @username";
                secondCommand = "UPDATE Users SET User_Username = @username WHERE User_ID = @id";
            }
            else return DB_FAILURE;


            MySqlCommand command = MakeCommand(firstCommand);
            command.Parameters.AddWithValue("@username", newUsername);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                connection.Close();
                return CREDENTIAL_TAKEN;
            }
            else
            {
                connection.Close();
                command = MakeCommand(secondCommand);
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@username", newUsername);
                command.Prepare();
                int result = command.ExecuteNonQuery();
                connection.Close();
                return result == 1 ? CREDENTIAL_CHANGE_SUCCESS : DB_FAILURE;
            }
        }

        public static int TryUpdateEmail(User user, string newEmail)
        {
            string firstCommand;
            string secondCommand;
            if (user.GetType() == typeof(Admin))
            {
                firstCommand = "SELECT Admin_ID FROM Admins WHERE Admin_Email = @email";
                secondCommand = "UPDATE Admins SET Admin_Email = @email WHERE Admin_ID = @id";
            }
            else if (user.GetType() == typeof(StandardUser))
            {
                firstCommand = "SELECT User_ID FROM Users WHERE User_Email = @email";
                secondCommand = "UPDATE Users SET User_Email = @email WHERE User_ID = @id";
            }
            else return DB_FAILURE;


            MySqlCommand command = MakeCommand(firstCommand);
            command.Parameters.AddWithValue("@email", newEmail);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                connection.Close();
                return CREDENTIAL_TAKEN;
            }
            else
            {
                connection.Close();
                command = MakeCommand(secondCommand);
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@email", newEmail);
                command.Prepare();
                int result = command.ExecuteNonQuery();
                connection.Close();
                return result == 1 ? CREDENTIAL_CHANGE_SUCCESS : DB_FAILURE;
            }
        }

        public static int UpdatePassword(User user, string newPassword)
        {
            string commandString;
            if (user.GetType() == typeof(Admin))
            {
                commandString = "UPDATE Admins SET Admin_Password = @password WHERE Admin_ID = @id";
            }
            else if (user.GetType() == typeof(StandardUser))
            {
                commandString = "UPDATE Users SET User_Password = @password WHERE User_ID = @id";
            }
            else return DB_FAILURE;


            MySqlCommand command = MakeCommand(commandString);
            command.Parameters.AddWithValue("@password", newPassword);
            command.Parameters.AddWithValue("@id", user.Id);
            command.Prepare();
            int result = command.ExecuteNonQuery();
            connection.Close();
            return result == 1 ? CREDENTIAL_CHANGE_SUCCESS : DB_FAILURE;
        }

        public static int UpdateProblem(Problem problem)
        {
            MySqlCommand command = MakeCommand("UPDATE Problems SET Problem_Title = @title, Problem_Answer = @answer, Problem_Attempts = @attempts, Problem_Completions = @completions WHERE Problem_Number = @id");
            command.Parameters.AddWithValue("@title", problem.Title);
            command.Parameters.AddWithValue("@answer", problem.Answer);
            command.Parameters.AddWithValue("@attempts", problem.Attempts);
            command.Parameters.AddWithValue("@completions", problem.Completions);
            command.Parameters.AddWithValue("@id", problem.ProblemNumber);
            command.Prepare();
            int result = command.ExecuteNonQuery();

            // update info in cached problem list
            if (result == 1)
            {
                for (int i = 0; i < problems.Count; i++)
                {
                    if (problems[i].ProblemNumber == problem.ProblemNumber)
                    {
                        problems[i] = problem;
                        break;
                    }
                }
            }

            return result;
        }

        public static List<AnswerSubmission> GetAnswerSubmissionsByID(int id) 
        {
            List<AnswerSubmission> subs = new List<AnswerSubmission>();
            //this query returns all of a users submissions, in ascending order by date
            string tablename = "UserSubmissions" + id;
            MySqlCommand command = MakeCommand("SELECT * FROM " + tablename);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            // need row count
            while (reader.Read())
            {
                // (string content, DateTime submissionDate, int id, bool isCorrect, int problemID)
                subs.Add(new AnswerSubmission(
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    id,
                    reader.GetBoolean(3),
                    reader.GetInt32(4)
                ));
            }
            connection.Close();
            return subs;
        }

        public static List<Submission> GetAdminSubmissionsByID(int id)
        {
            List<Submission> subs = new List<Submission>();
            MySqlCommand command = MakeCommand("SELECT * FROM AdminSubmissions WHERE Admin_ID = @id");
            command.Parameters.AddWithValue("@id", id);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            // need row count
            while (reader.Read())
            {
                subs.Add(new Submission(
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetInt32(1)
                ));
            }
            connection.Close();
            return subs;
        }

        public static Problem[] GetProblemsList()
        {
            problems = new List<Problem>();
            MySqlCommand command = MakeCommand("SELECT * FROM Problems");
            MySqlDataReader reader = command.ExecuteReader();
            // need row count
            while (reader.Read())
            {
                problems.Add(new Problem(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetInt32(5)
                ));
            }
            connection.Close();
            return problems.ToArray();
        }

        public static String[] GetCipherNameList()
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Ciphers");
            MySqlDataReader reader = command.ExecuteReader();
            List<String> ciphers = new List<String>();
            // need row count
            while (reader.Read())
            {
                ciphers.Add(reader.GetString(1));
            }
            connection.Close();
            return ciphers.ToArray();
        }

        public static bool InsertNewAdminSubmission(Submission submission)
        {
            MySqlCommand command = MakeCommand("INSERT INTO AdminSubmissions(Admin_ID, AdminSubmissions_Content, AdminSubmissions_SubmissionDate) VALUES(@id, @content, @date)");
            command.Parameters.AddWithValue("@id", submission.UserID);
            command.Parameters.AddWithValue("@content", submission.Content);
            command.Parameters.AddWithValue("@date", submission.SubmissionDate);
            command.Prepare();
            int result = command.ExecuteNonQuery();
            connection.Close();

            // submission added --> one row was affected (the added one)
            return result == 1;
        }

        public static bool InsertNewAnswerSubmission(AnswerSubmission submission)
        {
            string tablename = "UserSubmissions" + submission.UserID;
            MySqlCommand command = MakeCommand("INSERT INTO " + tablename + "(UserSubmissions_Answer, UserSubmissions_SubmissionDate, UserSubmissions_IsCorrect, UserSubmissions_ProblemID) VALUES(@content, @date, @correct, @problem_id)");
            command.Parameters.AddWithValue("@content", submission.Content);
            command.Parameters.AddWithValue("@date", submission.SubmissionDate);
            command.Parameters.AddWithValue("@correct", submission.IsCorrect);
            command.Parameters.AddWithValue("@problem_id", submission.ProblemId);
            command.Prepare();
            int result = command.ExecuteNonQuery();

            Problem problem = GetProblemByID(submission.ProblemId);

            //updates the correct field in the problem based on attempts vs. completions
            if (submission.IsCorrect)
            {
                problem.Completions++;
            }
            problem.Attempts++;


            //updates the problem in the DB
            UpdateProblem(problem);
            connection.Close();

            // submission added --> one row was affected (the added one)
            return result == 1;
        }

        public static Problem GetProblemByID(int ID)
        {


            MySqlCommand command = MakeCommand("SELECT * FROM Problems WHERE Problem_Number = @id");
            command.Parameters.AddWithValue("@id", ID);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();

            //if a problem was returned, read it and create a new problem to return
            if (reader.Read())
            {
                Problem problem = new Problem(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5)
                    );

                connection.Close();
                return problem;
            }

            //return an empty problem if a problem was not returned from the DB
            return new Problem();


        }


        public static int InsertNewUser(StandardUser user)
        {
            MySqlCommand command = MakeCommand("INSERT INTO Users(User_Username, User_Password, User_Email) VALUES(@username, @password, @email)");
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Prepare();
            int result = command.ExecuteNonQuery() == 1 ? (int)command.LastInsertedId : -1;
            connection.Close();

            // user added --> one row was affected (the added one)
            return result;
        }

        public static bool DoesEmailExist(string email)
        {
            MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Email = @email");
            command.Parameters.AddWithValue("@email", email);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();

            // user exists in standard users table
            if (reader.Read())
            {
                connection.Close();
                return true;
            }
            command = MakeCommand("SELECT Admin_ID FROM Admins WHERE Admin_Email = @email");
            command.Parameters.AddWithValue("@email", email);
            command.Prepare();
            reader = command.ExecuteReader();
            bool result = reader.Read();
            connection.Close();
            return result;
        }

        public static bool DoesUsernameExist(string username)
        {
            MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();

            // user exists in standard users table
            if (reader.Read())
            {
                connection.Close();
                return true;
            }
            command = MakeCommand("SELECT Admin_ID FROM Admins WHERE Admin_Username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            reader = command.ExecuteReader();
            bool result = reader.Read();
            connection.Close();
            return result;
        }

        public static StandardUser StandardUserLogin(string username, string password)
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username = @username AND User_Password = @password");
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                string usern = reader.GetString(1);
                string passw = reader.GetString(2);
                string email = reader.GetString(3);
                int rank = reader.GetInt32(4);
                int id = reader.GetInt32(0);

                Dictionary<int, List<AnswerSubmission>> submissionMap = new Dictionary<int, List<AnswerSubmission>>();
                //this will contain all of the users submissions in ascending order, by date, and then add them to the correct lists based on id
                List <AnswerSubmission> submissionList = GetAnswerSubmissionsByID(id);
                if (submissionList != null)
                {
                    foreach (AnswerSubmission answer in submissionList)
                    {
                        if (submissionMap.ContainsKey(answer.ProblemId))
                        {
                            submissionMap[answer.ProblemId].Add(answer);
                        }
                        else
                        {
                            List<AnswerSubmission> newSubList = new List<AnswerSubmission>();
                            newSubList.Add(answer);
                            submissionMap.Add(answer.ProblemId, newSubList);
                        }
                    }
                }

                //to check the values of the new submission map, as of right now it worksTM
                /*foreach(KeyValuePair<int, List<AnswerSubmission>> k in submissionMap)
                {
                    
                    foreach(AnswerSubmission a in k.Value)
                    {
                        Debug.Write(k.Key + " " + a.SubmissionDate + " " + a.Content + "\n");
                    }
                }*/
                connection.Close();
                return new StandardUser(
                    usern, passw, email, rank, id, submissionMap
                );
            }
            else
            {
                connection.Close();
                return new StandardUser();
            }
        }

        public static Admin AdminLogin(string username, string password)
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Admins WHERE Admin_Username = @username AND Admin_Password = @password");
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                // TODO must get submissions for admin before returning
                string usern = reader.GetString(1);
                string passw = reader.GetString(2);
                string email = reader.GetString(3);
                int id = reader.GetInt32(0);
                connection.Close();
                return new Admin(
                    usern,
                    passw,
                    email,
                    id
                );
            }
            else
            {
                connection.Close();
                return new Admin();
            }
        }

        public static bool DeleteUser(string username)
        {
            MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                int userID = reader.GetInt32(0);
                connection.Close();
                string tablename = "UserSubmissions" + userID;
                command = MakeCommand("SELECT * FROM " + tablename);
                command.Parameters.AddWithValue("@id", userID);
                command.Prepare();
                reader = command.ExecuteReader();
                Dictionary<int, int[]> submissionInfos = new Dictionary<int, int[]>();
                while (reader.Read())
                {
                    int problemID = reader.GetInt32(4);
                    bool isCorrect = reader.GetBoolean(3);
                    if (submissionInfos.ContainsKey(problemID))
                    {
                        int[] attemptsAndCompletions = submissionInfos[problemID];
                        attemptsAndCompletions[0]++;
                        if (isCorrect)
                        {
                            attemptsAndCompletions[1]++;
                        }
                        submissionInfos[problemID] = attemptsAndCompletions;
                    }
                    else 
                    {
                        int[] newProblem = new int[2];
                        newProblem[0] = 1;
                        newProblem[1] = isCorrect ? 1 : 0;
                        submissionInfos.Add(problemID, newProblem);
                    }
                }
                command.Parameters.Clear();
                reader.Close();
                command.CommandText = @"
                                        UPDATE Problems SET Problem_Attempts = Problem_Attempts - @attempts, Problem_Completions = Problem_Completions - @completions
                                        WHERE Problem_Number = @pID";
                foreach (int pID in submissionInfos.Keys)
                {
                    int[] aandc = submissionInfos[pID];
                    command.Parameters.AddWithValue("@attempts", aandc[0]);
                    command.Parameters.AddWithValue("@completions", aandc[1]);
                    command.Parameters.AddWithValue("@pID", pID);
                    command.Prepare();
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }

                // remove user's submissions
                command.CommandText = "DROP TABLE " + tablename;
                command.Prepare();
                command.ExecuteNonQuery();
                command.Parameters.Clear();

                // remove user
                command.CommandText = "DELETE FROM Users WHERE User_ID = @id";
                command.Parameters.AddWithValue("@id", userID);
                command.Prepare();
                int result = command.ExecuteNonQuery();
                connection.Close();

                // refresh problems attempt / completion count the bad way because time crunch
                GetProblemsList();

                // user deleted --> one row was affected (the deleted one)
                return result == 1;
            }
            else
            {
                return false;
            }
        }

        public static StandardUser GetStandardUserByUsername(string username) 
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                string usern = reader.GetString(1);
                string passw = reader.GetString(2);
                string email = reader.GetString(3);
                int rank = reader.GetInt32(4);
                int id = reader.GetInt32(0);
                connection.Close();
                return new StandardUser(
                    usern, passw, email, rank, id
                );
            }
            else
            {
                connection.Close();
                return new StandardUser();
            }
        }

        public static StandardUser GetStandardUserByEmail(string email)
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Email = @email");
            command.Parameters.AddWithValue("@email", email);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                string usern = reader.GetString(1);
                string passw = reader.GetString(2);
                int rank = reader.GetInt32(4);
                int id = reader.GetInt32(0);
                connection.Close();
                return new StandardUser(
                    usern, passw, email, rank, id
                );
            }
            else
            {
                connection.Close();
                return new StandardUser();
            }
        }

    }
}
