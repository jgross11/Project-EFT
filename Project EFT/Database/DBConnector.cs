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
        public static int UserCount;

        public const int DB_FAILURE = -1;
        public const int CREDENTIAL_CHANGE_SUCCESS = 100;
        public const int CREDENTIAL_TAKEN = 101;
        public const int EXPECTED_THREADS_CONNECTED = 2;

        public static void Init()
        {
            string[] lines = System.IO.File.ReadAllLines("info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];

            // cache list of all problems currently in db
            if (!GetProblemsList()) throw new Exception("Could not read initial problem list from DB");

            // cache number of users in db
            if (!GetNumberUsersInDB()) throw new Exception("Could not get initial number of users in DB");
        }

        public static void InitForTests()
        {

            string[] lines = System.IO.File.ReadAllLines("../../../../Project EFT/info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];


            MySqlCommand command = MakeCommand("SHOW STATUS LIKE 'Threads_Connected'");
            
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine(reader.GetInt32(1));
                if (reader.GetInt32(1) != EXPECTED_THREADS_CONNECTED)
                {
                    connection.Close();
                    throw new Exception("Shouldn't run database tests since there are users connected to the DB");
                }
            }
            reader.Close();
            connection.Close();

            // cache list of all problems currently in db
            if (!GetProblemsList()) throw new Exception("Could not read initial problem list from DB");

            // cache number of users in db
            if (!GetNumberUsersInDB()) throw new Exception("Could not get initial number of users in DB");
        }

        public static bool OpenConnection()
        {
            // dependent on the structure of the file...
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static MySqlCommand MakeCommand(string statement)
        {
            return OpenConnection() ? new MySqlCommand(statement, connection) : null;
        }

        public static MySqlTransaction MakeTransaction()
        {
            if (OpenConnection())
                try
                {
                    return connection.BeginTransaction();
                }
                catch 
                {
                    if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                    return null;
                }
            return null;
        }

        public static bool CheckIfUserSubmissionTableExists(int UserId)
        {
            string tablename = "UserSubmissions" + UserId;
            MySqlCommand command = MakeCommand("SELECT * FROM " + tablename);
            MySqlDataReader reader = null;
            try
            {
                command.Prepare();
                reader = command.ExecuteReader();
                connection.Close();
                reader.Close();
                return true;
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
                return false;
            }
        }

        public static bool ResetProblemSubmissions(StandardUser user, int problemID)
        {
            if (CheckIfUserSubmissionTableExists(user.Id))
            {
                MySqlTransaction transaction = MakeTransaction();
                MySqlDataReader reader = null;
                if (transaction != null) 
                {
                    try
                    {
                        string tablename = "UserSubmissions" + user.Id;
                        MySqlCommand command = new MySqlCommand("SELECT * FROM " + tablename + " WHERE UserSubmissions_ProblemID = @problemNumber", connection, transaction);
                        command.Parameters.AddWithValue("@problemNumber", problemID);
                        command.Prepare();
                        reader = command.ExecuteReader();
                        int numAttempts = 0;
                        int numCompletions = 0;
                        while (reader.Read())
                        {
                            bool isCorrect = reader.GetBoolean(3);
                            numAttempts++;
                            if (reader.GetBoolean(3)) numCompletions++;
                        }
                        command.Parameters.Clear();
                        reader.Close();
                        command.CommandText = @"
                                        UPDATE Problems SET Problem_Attempts = Problem_Attempts - @attempts, Problem_Completions = Problem_Completions - @completions
                                        WHERE Problem_Number = @pID";
                        command.Parameters.AddWithValue("@attempts", numAttempts);
                        command.Parameters.AddWithValue("@completions", numCompletions);
                        command.Parameters.AddWithValue("@pID", problemID);
                        command.Prepare();
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();

                        command.CommandText = "DELETE FROM " + tablename + " WHERE UserSubmissions_ProblemID = @pID";
                        command.Parameters.AddWithValue("@pID", problemID);
                        command.Prepare();
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                        command.CommandText = "SELECT Problem_PointValue FROM Problems WHERE Problem_Number = @pID";
                        command.Parameters.AddWithValue("@pID", problemID);
                        command.Prepare();
                        reader = command.ExecuteReader();
                        int pointValue = 0;
                        if (reader.Read()) pointValue = reader.GetInt32(0);
                        else throw new Exception("No value returned when finding problem worth");
                        command.Parameters.Clear();
                        reader.Close();
                        command.CommandText = "UPDATE Users SET User_PointsTotal = User_PointsTotal - @value WHERE User_ID = @id";
                        command.Parameters.AddWithValue("@value", pointValue);
                        command.Parameters.AddWithValue("@id", user.Id);
                        command.Prepare();
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.CommandText = @"UPDATE Users SET User_Ranking = User_Ranking - 1 
                                    WHERE ((User_PointsTotal < @currentValue AND User_PointsTotal >= @newValue) OR
                                    (User_PointsTotal = @currentValue AND User_Ranking > @currentRank)) AND User_ID != @id";
                        command.Parameters.AddWithValue("@currentValue", user.PointsTotal);
                        command.Parameters.AddWithValue("@newValue", user.PointsTotal - pointValue);
                        command.Parameters.AddWithValue("@id", user.Id);
                        command.Parameters.AddWithValue("@currentRank", user.Ranking);
                        command.Prepare();
                        int rankingDifference = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Users SET User_Ranking = User_Ranking + @difference WHERE User_ID = @id";
                        command.Parameters.AddWithValue("@difference", rankingDifference);
                        command.Parameters.AddWithValue("@id", user.Id);
                        command.Prepare();
                        command.ExecuteNonQuery();

                        transaction.Commit();
                        connection.Close();

                        // refresh problems attempt / completion count the bad way because time crunch
                        GetProblemsList();

                        return true;
                    }
                    catch
                    {
                        if (connection != null && connection.State == ConnectionState.Open)
                        {
                            if(transaction != null) transaction.Rollback();
                            connection.Close();
                        }
                        if (reader != null && !reader.IsClosed) reader.Close();
                    }
                }
            }
            return false;
        }

        public static bool CreateUserSubmissionTable(int ID)
        {
            try
            {
                string tablename = "UserSubmissions" + ID;
                MySqlCommand command = MakeCommand("CREATE TABLE " + tablename + " (UserSubmissions_ID INT NOT NULL AUTO_INCREMENT, UserSubmissions_Answer VARCHAR(255) NOT NULL, UserSubmissions_SubmissionDate DATETIME NULL, UserSubmissions_IsCorrect TINYINT NOT NULL, UserSubmissions_ProblemID INT NOT NULL, PRIMARY KEY(UserSubmissions_ID))");
                command.Prepare();
                command.ExecuteNonQuery();
                connection.Close();
                return true;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return false;
        }
        
        public static bool InsertNewProblem(Problem problem)
        {
            try
            {
                MySqlCommand command = MakeCommand("INSERT INTO Problems (Problem_Title, Problem_Question, Problem_Answer, Problem_PointValue, Problem_Number) VALUES (@title, @question, @answer, @pointValue, @problemnumber)");
                command.Parameters.AddWithValue("@title", problem.Title);
                command.Parameters.AddWithValue("@question", problem.Question);
                command.Parameters.AddWithValue("@answer", problem.Answer);
                command.Parameters.AddWithValue("@pointValue", problem.PointsValue);
                command.Parameters.AddWithValue("@problemnumber", problems.Count + 1);
                command.Prepare();
                int numRowsAffected = command.ExecuteNonQuery();
                connection.Close();

                if (numRowsAffected == 1)
                {
                    problem.ProblemNumber = problems.Count + 1;
                    problems.Add(problem);
                    return true;
                }
                return false;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return false;
        }

        public static int TryUpdateUsername(User user, string newUsername)
        {
            MySqlDataReader reader = null;
            try
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
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    connection.Close();
                    reader.Close();
                    return CREDENTIAL_TAKEN;
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    command = MakeCommand(secondCommand);
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@username", newUsername);
                    command.Prepare();
                    int result = command.ExecuteNonQuery();
                    connection.Close();
                    return result == 1 ? CREDENTIAL_CHANGE_SUCCESS : DB_FAILURE;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return DB_FAILURE;
        }

        public static int TryUpdateEmail(User user, string newEmail)
        {
            MySqlDataReader reader = null;
            try
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
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    connection.Close();
                    reader.Close();
                    return CREDENTIAL_TAKEN;
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    command = MakeCommand(secondCommand);
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@email", newEmail);
                    command.Prepare();
                    int result = command.ExecuteNonQuery();
                    connection.Close();
                    return result == 1 ? CREDENTIAL_CHANGE_SUCCESS : DB_FAILURE;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return DB_FAILURE;
        }

        public static int UpdatePassword(User user, string newPassword)
        {
            try
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
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return DB_FAILURE;
        }

        public static bool UpdateAbout(string newAbout, int Id)
        {
            MySqlCommand command = MakeCommand("UPDATE Users SET User_About = @newAbout WHERE User_ID = @id");
            try
            {
                command.Parameters.AddWithValue("@newAbout", newAbout);
                command.Parameters.AddWithValue("@id", Id);
                command.Prepare();
                int result = command.ExecuteNonQuery();
                connection.Close();
                return result == 1;
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return false;
        }

        public static int UpdateProblem(Problem problem)
        {
            try
            {
                MySqlCommand command = MakeCommand("UPDATE Problems SET Problem_Title = @title, Problem_Question = @question, Problem_Answer = @answer, Problem_Attempts = @attempts, Problem_Completions = @completions, Problem_PointValue = @value WHERE Problem_Number = @id");
                command.Parameters.AddWithValue("@title", problem.Title);
                command.Parameters.AddWithValue("@question", problem.Question);
                command.Parameters.AddWithValue("@answer", problem.Answer);
                command.Parameters.AddWithValue("@attempts", problem.Attempts);
                command.Parameters.AddWithValue("@completions", problem.Completions);
                command.Parameters.AddWithValue("@value", problem.PointsValue);
                command.Parameters.AddWithValue("@id", problem.ProblemNumber);
                command.Prepare();
                int result = command.ExecuteNonQuery();

                // update info in cached problem list
                if (result == 1)
                {
                    problems[problem.ProblemNumber - 1] = problem;
                }
                connection.Close();
                return result;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return 0;
        }

        public static List<AnswerSubmission> GetAnswerSubmissionsByID(int id)
        {
            MySqlDataReader reader = null;
            try
            {
                List<AnswerSubmission> subs = new List<AnswerSubmission>();
                //this query returns all of a users submissions, in ascending order by date
                string tablename = "UserSubmissions" + id;
                MySqlCommand command = MakeCommand("SELECT * FROM " + tablename + " ORDER BY UserSubmissions_SubmissionDate ASC");
                command.Prepare();
                reader = command.ExecuteReader();
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
                reader.Close();
                connection.Close();
                return subs;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static List<Submission> GetAdminSubmissionsByID(int id)
        {
            MySqlDataReader reader = null;
            try
            {
                List<Submission> subs = new List<Submission>();
                MySqlCommand command = MakeCommand("SELECT * FROM AdminSubmissions WHERE Admin_ID = @id  ORDER BY AdminSubmissions_SubmissionDate ASC");
                command.Parameters.AddWithValue("@id", id);
                command.Prepare();
                reader = command.ExecuteReader();
                // need row count
                while (reader.Read())
                {
                    subs.Add(new Submission(
                        reader.GetString(2),
                        reader.GetDateTime(3),
                        reader.GetInt32(1)
                    ));
                }
                reader.Close();
                connection.Close();
                return subs;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static bool GetProblemsList()
        {
            MySqlDataReader reader = null;
            try
            {
                problems = new List<Problem>();
                MySqlCommand command = MakeCommand("SELECT * FROM Problems ORDER BY Problem_Number ASC");
                reader = command.ExecuteReader();
                // need row count
                while (reader.Read())
                {
                    problems.Add(new Problem(
                        reader.GetInt32(7),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5),
                        reader.GetInt32(6)
                    ));
                }
                reader.Close();
                connection.Close();
                return true;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return false;
        }

        public static bool InsertNewAdminSubmission(Submission submission)
        {
            try
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
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return false;
        }

        public static int GetUserRanking(int userID) 
        {
            MySqlCommand command = MakeCommand("SELECT User_Ranking FROM Users WHERE User_ID = @id");
            MySqlDataReader reader = null;
            try
            {
                command.Parameters.AddWithValue("@id", userID);
                command.Prepare();
                reader = command.ExecuteReader();
                int result = DB_FAILURE;
                if (reader.Read()) result = reader.GetInt32(0);
                connection.Close();
                reader.Close();
                return result;
            }
            catch (Exception e){
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return DB_FAILURE;
        }

        public static bool InsertNewAnswerSubmission(AnswerSubmission submission, int currentUserPointsTotal, int problemWorth)
        {
            MySqlTransaction transaction = MakeTransaction();
            try
            {
                string tablename = "UserSubmissions" + submission.UserID;
                MySqlCommand command = new MySqlCommand(
                    "INSERT INTO " + tablename + "(UserSubmissions_Answer, UserSubmissions_SubmissionDate, UserSubmissions_IsCorrect, UserSubmissions_ProblemID) VALUES(@content, @date, @correct, @problem_id)",
                    connection, transaction
                    );
                command.Parameters.AddWithValue("@content", submission.Content);
                command.Parameters.AddWithValue("@date", submission.SubmissionDate);
                command.Parameters.AddWithValue("@correct", submission.IsCorrect);
                command.Parameters.AddWithValue("@problem_id", submission.ProblemId);
                command.Prepare();
                int result = command.ExecuteNonQuery();

                Problem problem = null;
                foreach (Problem p in problems) 
                {
                    if (p.ProblemNumber == submission.ProblemId)
                    {
                        problem = p;
                        break;
                    }
                }
                if(problem != null) {
                    //updates the correct field in the problem based on attempts vs. completions
                    if (submission.IsCorrect)
                    {
                        problem.Completions++;
                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Users SET User_Ranking = User_Ranking + 1 WHERE User_PointsTotal >= @currentTotal AND User_PointsTotal < @newTotal AND User_ID != @id";
                        command.Parameters.AddWithValue("@currentTotal", currentUserPointsTotal);
                        command.Parameters.AddWithValue("@newTotal", currentUserPointsTotal + problemWorth);
                        command.Parameters.AddWithValue("@id", submission.UserID);
                        command.Prepare();
                        int numRowsAffected = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Users SET User_Ranking = User_Ranking - @ratingDifference WHERE User_ID = @id";
                        command.Parameters.AddWithValue("@ratingDifference", numRowsAffected);
                        command.Parameters.AddWithValue("@id", submission.UserID);
                        command.Prepare();
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.CommandText = "UPDATE Users SET User_PointsTotal = User_PointsTotal + @pointsValue WHERE User_ID = @id";
                        command.Parameters.AddWithValue("@pointsValue", problem.PointsValue);
                        command.Parameters.AddWithValue("@id", submission.UserID);
                        command.Prepare();
                        command.ExecuteNonQuery();
                    }
                    problem.Attempts++;


                    command.Parameters.Clear();
                    command.CommandText = "UPDATE Problems SET Problem_Attempts = @attempts, Problem_Completions = @completions WHERE Problem_Number = @id";
                    command.Parameters.AddWithValue("@attempts", problem.Attempts);
                    command.Parameters.AddWithValue("@completions", problem.Completions);
                    command.Parameters.AddWithValue("@id", problem.ProblemNumber);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    connection.Close();

                    // submission added --> one row was affected (the added one)
                    return true;
                }
            }
            catch (Exception e)
            {
                if (connection != null && connection.State == ConnectionState.Open) 
                {
                    if (transaction != null) transaction.Rollback();
                    connection.Close();
                }
                Debug.Write(e);
            }
            return false;
        }

        public static Problem GetProblemByID(int ID)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Problems WHERE Problem_Number = @id");
                command.Parameters.AddWithValue("@id", ID);
                command.Prepare();
                reader = command.ExecuteReader();

                //if a problem was returned, read it and create a new problem to return
                if (reader.Read())
                {
                    Problem problem = new Problem(
                            reader.GetInt32(7),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetInt32(4),
                            reader.GetInt32(5),
                            reader.GetInt32(6)
                        );

                    connection.Close();
                    reader.Close();
                    return problem;
                }

                connection.Close();
                reader.Close();
                return null;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static int InsertNewUser(StandardUser user)
        {
            try
            {
                MySqlCommand command = MakeCommand("INSERT INTO Users(User_Username, User_Password, User_Email, User_Ranking) VALUES(@username, @password, @email, @ranking)");
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@ranking", ++UserCount);
                command.Prepare();
                int result = command.ExecuteNonQuery() == 1 ? (int)command.LastInsertedId : -1;
                connection.Close();
                return result;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return -1;
        }

        public static bool DoesEmailExist(string email)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Email = @email");
                command.Parameters.AddWithValue("@email", email);
                command.Prepare();
                reader = command.ExecuteReader();

                // user exists in standard users table
                if (reader.Read())
                {
                    reader.Close();
                    connection.Close();
                    return true;
                }
                command.CommandText = "SELECT Admin_ID FROM Admins WHERE Admin_Email = @email";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@email", email);
                command.Prepare();
                reader.Close();
                reader = command.ExecuteReader();
                bool result = reader.Read();
                connection.Close();
                reader.Close();
                return result;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return false;
        }

        public static bool DoesUsernameExist(string username)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Username = @username");
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                reader = command.ExecuteReader();

                // user exists in standard users table
                if (reader.Read())
                {
                    reader.Close();
                    connection.Close();
                    return true;
                }
                command.CommandText = "SELECT Admin_ID FROM Admins WHERE Admin_Username = @username";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                reader.Close();
                reader = command.ExecuteReader();
                bool result = reader.Read();
                connection.Close();
                reader.Close();
                return result;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return false;
        }

        public static StandardUser StandardUserLogin(string username, string password)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username = @username AND User_Password = @password");
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    string email = reader.GetString(3);
                    int rank = reader.GetInt32(4);
                    int points = reader.GetInt32(5);
                    int id = reader.GetInt32(0);
                    string about = reader.GetString(6);
                    connection.Close();
                    reader.Close();
                    Dictionary<int, List<AnswerSubmission>> submissionMap = new Dictionary<int, List<AnswerSubmission>>();
                    //this will contain all of the users submissions in ascending order, by date, and then add them to the correct lists based on id
                    if (CheckIfUserSubmissionTableExists(id))
                    {
                        List<AnswerSubmission> submissionList = GetAnswerSubmissionsByID(id);
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
                                    List<AnswerSubmission> newSubList = new List<AnswerSubmission>() { answer };
                                    submissionMap.Add(answer.ProblemId, newSubList);
                                }
                            }
                        }
                        else submissionMap = null;
                    }
                    //to check the values of the new submission map, as of right now it worksTM
                    /*foreach(KeyValuePair<int, List<AnswerSubmission>> k in submissionMap)
                    {

                        foreach(AnswerSubmission a in k.Value)
                        {
                            Debug.Write(k.Key + " " + a.SubmissionDate + " " + a.Content + "\n");
                        }
                    }*/
                    StandardUser result = new StandardUser(
                        usern, passw, email, rank, points, id, submissionMap
                    );
                    result.About = about;

                    return result;
                }
                else
                {
                    reader.Close();
                    connection.Close();
                    return null;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static Admin AdminLogin(string username, string password)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Admins WHERE Admin_Username = @username AND Admin_Password = @password");
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // TODO must get submissions for admin before returning
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    string email = reader.GetString(3);
                    int id = reader.GetInt32(0);
                    connection.Close();
                    reader.Close();
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
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static bool DeleteUser(string username)
        {
            StandardUser user = GetStandardUserByUsername(username);
            MySqlTransaction transaction = null;
            if (user == null) return false;
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = null;
                //if they have a user submissions table, remove their submissions and then drop the table, otherwise just delete the user
                if (CheckIfUserSubmissionTableExists(user.Id))
                {
                    transaction = MakeTransaction();
                    if (transaction != null)
                    {
                        string tablename = "UserSubmissions" + user.Id;
                        command = new MySqlCommand("SELECT * FROM " + tablename, connection, transaction);
                        command.Parameters.AddWithValue("@id", user.Id);
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
                    }
                }

                // remove user
                if (connection.State != ConnectionState.Open) transaction = MakeTransaction();
                command = new MySqlCommand("DELETE FROM Users WHERE User_ID = @id", connection, transaction);
                command.Parameters.AddWithValue("@id", user.Id);
                command.Prepare();
                command.ExecuteNonQuery();
                command.Parameters.Clear();
                command.CommandText = "UPDATE Users SET User_Ranking = User_Ranking - 1 WHERE User_Ranking > @ranking";
                command.Parameters.AddWithValue("@ranking", user.Ranking);
                command.Prepare();
                command.ExecuteNonQuery();

                transaction.Commit();
                connection.Close();

                // refresh problems attempt / completion count the bad way because time crunch
                GetProblemsList();

                UserCount--;

                if (File.Exists(InformationValidator.ImageProjectPath + "/" + user.Id + ".png")) File.Delete(InformationValidator.ImageProjectPath + "/" + user.Id + ".png");

                // user deleted --> one row was affected (the deleted one)
                return true;
            }

            catch
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    if (transaction != null) transaction.Rollback();
                    connection.Close();
                }
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return false;
        }

        public static StandardUser GetStandardUserByUsername(string username) 
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username = @username");
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    string email = reader.GetString(3);
                    int rank = reader.GetInt32(4);
                    int id = reader.GetInt32(0);
                    int points = reader.GetInt32(5);
                    string about = reader.GetString(6);
                    connection.Close();
                    reader.Close();
                    return new StandardUser(
                        usern, passw, email, rank, points, id, about
                    );
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static StandardUser GetStandardUserByEmail(string email)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Email = @email");
                command.Parameters.AddWithValue("@email", email);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    int rank = reader.GetInt32(4);
                    int id = reader.GetInt32(0);
                    int points = reader.GetInt32(5);
                    string about = reader.GetString(6);
                    connection.Close();
                    reader.Close();
                    return new StandardUser(
                        usern, passw, email, rank, points, id, about
                    );
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        //currently only for tests
        public static Admin GetAdminByUsername(string username)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Admins WHERE Admin_Username = @username");
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    string email = reader.GetString(3);
                    connection.Close();
                    reader.Close();
                    return new Admin(
                        usern, passw, email, id
                    );
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        //currently only for tests
        public static Admin GetAdminByEmail(string email)
        {
            MySqlDataReader reader = null;
            try
            {
                MySqlCommand command = MakeCommand("SELECT * FROM Admins WHERE Admin_Email = @email");
                command.Parameters.AddWithValue("@email", email);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string usern = reader.GetString(1);
                    string passw = reader.GetString(2);
                    connection.Close();
                    reader.Close();
                    return new Admin(
                        usern, passw, email, id
                    );
                }
                else
                {
                    connection.Close();
                    reader.Close();
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static bool GetNumberUsersInDB() 
        {
            MySqlCommand command = MakeCommand("SELECT COUNT(User_ID) FROM Users");
            MySqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    UserCount = reader.GetInt32(0);
                    connection.Close();
                    reader.Close();
                    return true;
                }
                connection.Close();
                reader.Close();
            }
            catch 
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return false;
        }

        public static List<StandardUser> GetTopFiveUsers() 
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Ranking < 6 ORDER BY User_Ranking ASC");
            MySqlDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    List<StandardUser> result = new List<StandardUser>();
                    do
                    {
                        StandardUser user = new StandardUser();
                        user.Id = reader.GetInt32(0);
                        user.Username = reader.GetString(1);
                        user.Ranking = reader.GetInt32(4);
                        user.PointsTotal = reader.GetInt32(5);
                        result.Add(user);
                    } while (reader.Read());
                    connection.Close();
                    reader.Close();
                    return result;
                }
                connection.Close();
                reader.Close();
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static StandardUser GetUserProfileInformationByUsername(string username)
        {
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username = @username");
            MySqlDataReader reader = null;
            try
            {
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    StandardUser user = new StandardUser();
                    user.Id = reader.GetInt32(0);
                    user.Username = reader.GetString(1);
                    user.Ranking = reader.GetInt32(4);
                    user.PointsTotal = reader.GetInt32(5);
                    user.About = reader.GetString(6);
                    connection.Close();
                    reader.Close();
                    if (CheckIfUserSubmissionTableExists(user.Id))
                    {
                        List<AnswerSubmission> submissionList = GetAnswerSubmissionsByID(user.Id);
                        Dictionary<int, List<AnswerSubmission>> submissionMap = new Dictionary<int, List<AnswerSubmission>>();
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
                                    List<AnswerSubmission> newSubList = new List<AnswerSubmission>() { answer };
                                    submissionMap.Add(answer.ProblemId, newSubList);
                                }
                            }
                        }
                        else submissionMap = null;
                        user.Submissions = submissionMap;
                    }
                    return user;
                }
                connection.Close();
                reader.Close();
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        public static List<StandardUser> SearchForUser(string query) 
        {
            string relativeQuery = '%' + query + '%';
            MySqlCommand command = MakeCommand("SELECT * FROM Users WHERE User_Username LIKE @query ORDER BY User_Ranking ASC");
            MySqlDataReader reader = null;
            try
            {
                command.Parameters.AddWithValue("@query", relativeQuery);
                command.Prepare();
                reader = command.ExecuteReader();
                if (reader.Read()) 
                {
                    List<StandardUser> results = new List<StandardUser>();
                    do
                    {
                        StandardUser user = new StandardUser();
                        user.Id = reader.GetInt32(0);
                        user.Username = reader.GetString(1);
                        user.Ranking = reader.GetInt32(4);
                        user.PointsTotal = reader.GetInt32(5);
                        results.Add(user);
                    } while (reader.Read());
                    connection.Close();
                    reader.Close();
                    return results;
                }
                connection.Close();
                reader.Close();
                return null;
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        //currently only used for tests
        public static void DeleteAdminSubmissionByContent(string content)
        {
            MySqlTransaction transaction = MakeTransaction();
            if (transaction != null)
            {
                try
                {
                    MySqlCommand command = new MySqlCommand("DELETE FROM AdminSubmissions WHERE AdminSubmissions_Content = @content", connection, transaction);
                    command.Parameters.AddWithValue("@content", content);
                    command.Prepare();
                    command.ExecuteNonQuery();
                    
                    transaction.Commit();
                    connection.Close();
                }
                catch
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        if (transaction != null) transaction.Rollback();
                        connection.Close();
                    }
                }
            }
        }

        //currently only used for tests
        public static void DeleteAnswerSubmissionByAnswer(string answer, int userID)
        {
            //if this occurs *after* the transaction is made, the connection is closed....
            if (CheckIfUserSubmissionTableExists(userID))
            {
                MySqlTransaction transaction = MakeTransaction();
                if (transaction != null)
                {
                    try
                    {
                        string tableName = "UserSubmissions" + userID;
                        MySqlCommand command = new MySqlCommand("DELETE FROM " + tableName + " WHERE UserSubmissions_Answer = @answer", connection, transaction);
                        command.Parameters.AddWithValue("@answer", answer);
                        command.Prepare();
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        connection.Close();
                    }
                    catch
                    {
                        if (connection != null && connection.State == ConnectionState.Open)
                        {
                            if (transaction != null) transaction.Rollback();
                            connection.Close();
                        }
                    }
                }
            }
        }

        //currently only used for tests, but could be added into "deleteUser()" query
       public static void DropUserSubmissionTable(int tableID)
        {
            MySqlTransaction transaction = MakeTransaction();
            if (transaction != null)
            {
                try
                {
                    string tableName = "UserSubmissions" + tableID;
                    MySqlCommand command = new MySqlCommand("DROP TABLE " + tableName, connection, transaction);
                    command.Prepare();
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    connection.Close();
                }
                catch
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        if (transaction != null) transaction.Rollback();
                        connection.Close();
                    }
                }
            }
        }

        //currently only used for tests, but could be used by admin with some adjustments to update all problems in front's id...updateProblem()
        public static void DeleteProblemByID(int problemID)
        {
            MySqlTransaction transaction = MakeTransaction();
            if (transaction != null)
            {
                try
                {
                    MySqlCommand command = new MySqlCommand("DELETE FROM Problems WHERE Problem_Number = @pID", connection, transaction);
                    command.Parameters.AddWithValue("@pID", problemID);
                    command.Prepare();
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    connection.Close();
                }
                catch
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                    {
                        if (transaction != null) transaction.Rollback();
                        connection.Close();
                    }
                }
            }
        }

    }
}
