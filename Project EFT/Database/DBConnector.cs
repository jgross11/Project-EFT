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
    /// <summary>Every DB CRUD operation is done through this class. Good luck.</summary>
    public class DBConnector
    {
        /// <summary>Connection to the DB that probably shouldn't be static or referenced by every query.</summary>
        public static MySqlConnection connection;

        /// <summary>URL/URI string that contains the DB connection information.</summary>
        public static string connectionString;

        /// <summary>All problems in the database that probably shouldn't be static.</summary>
        public static List<Problem> problems;

        /// <summary>The number of <see cref="StandardUser"/>s in the DB.</summary>
        public static int UserCount;

        /// <summary>Indicates that the DB encountered a problem when executing some query, for queries that need more information than just true / false responses.</summary>
        public const int DB_FAILURE = -1;

        /// <summary>Indicates that the credential change was successful, for credential updating queries.</summary>
        public const int CREDENTIAL_CHANGE_SUCCESS = 100;

        /// <summary>Indicates that the credential change failed because they belong to another account, for credential updating queries.</summary>
        public const int CREDENTIAL_TAKEN = 101;

        /// <summary>Number of threads that should be connected to the DB when DB tests are ran so as to not allow concurrency wonkiness.</summary>
        public const int EXPECTED_THREADS_CONNECTED = 2;

        /// <summary>Called when the server is first started. <br/>
        /// Reads credential information from the credentials file and generates connection string. <br/>
        /// Reads problem information in DB into cache. <br/>
        /// Reads the number of users in DB into cache.</summary>
        public static void Init()
        {
            string[] lines = System.IO.File.ReadAllLines("info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];

            // cache list of all problems currently in db
            if (!GetProblemsList()) throw new Exception("Could not read initial problem list from DB");

            // cache number of users in db
            if (!GetNumberUsersInDB()) throw new Exception("Could not get initial number of users in DB");
        }

        /// <summary>Called when DB tests are started.<br/>
        /// Ensures there are the right number of threads connected to the DB before DB tests can run. <br/>
        /// If DB tests shouldn't run, an exception is thrown to prevent them from running. <br/>
        /// Otherwise, formats connection string in preparation for DB tests.</summary>
        public static void InitForTests()
        {

            string[] lines = System.IO.File.ReadAllLines("../../../../Project EFT/info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];

            // SHOW PROCESSLIST for more info
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

        /// <summary>Open a connection to the DB.</summary>
        /// <returns>True if a connection was successfully opened. False otherwise.</returns>
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

        /// <summary>Makes a Command with the given MySQL statement.</summary>
        /// <param name="statement">The MySQL statement to create.</param>
        /// <returns>A <see cref="MySqlCommand"/> representing the given MySQL statement if a connection could be opened. Null otherwise.</returns>
        public static MySqlCommand MakeCommand(string statement)
        {
            return OpenConnection() ? new MySqlCommand(statement, connection) : null;
        }

        /// <summary>Makes a Transaction to be used for a series of CRUD queries.</summary>
        /// <returns>A <see cref="MySqlTransaction"/> representing the created transaction, if a connection and transaction could be created. Null otherwise.</returns>
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

        /// <summary>Determines if the given <see cref="StandardUser"/>'s submission table exists.</summary>
        /// <param name="UserId">ID of the <see cref="StandardUser"/> whose submission table will be queried for.</param>
        /// <returns>True if the <see cref="StandardUser"/>'s submission table exists. False otherwise.</returns>
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

        /// <summary>Resets a specific <see cref="Problem"/>'s <see cref="AnswerSubmission"/>s for a specific <see cref="StandardUser"/>.</summary>
        /// <param name="user"><see cref="StandardUser"/> whose submissions will be reset.</param>
        /// <param name="problemID">Problem number of the <see cref="Problem"/> whose submissions will be deleted.</param>
        /// <returns>True if the problem's submissions are reset, user rank and point total are updated, and problem attempts and completions are updated successfully. False otherwise.</returns>
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

        /// <summary>Creates an <see cref="AnswerSubmission"/> table for a <see cref="StandardUser"/>.</summary>
        /// <param name="ID">ID of the <see cref="StandardUser"/> whose submission table will be created.</param>
        /// <returns>True if the submission table was successfully created. False otherwise.</returns>
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
        
        /// <summary>Submits the given <see cref="Problem"/> information into the DB.</summary>
        /// <param name="problem">Problem to insert into DB.</param>
        /// <returns>True if the problem was successfully added to the DB. False otherwise.</returns>
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

        /// <summary>Updates the given <see cref="User"/>'s username.</summary>
        /// <param name="user"><see cref="User"/> whose username will be updated.</param>
        /// <param name="newUsername">The new username.</param>
        /// <returns><see cref="CREDENTIAL_CHANGE_SUCCESS"/> if the <see cref="User"/>'s username was updated in the DB,<br/>
        /// <see cref="CREDENTIAL_TAKEN"/> if the username is taken by another account of the same type, and <br/>
        /// <see cref="DB_FAILURE"/> otherwise.</returns>
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

        /// <summary>Updates the given <see cref="User"/>'s email.</summary>
        /// <param name="user"><see cref="User"/> whose email will be updated.</param>
        /// <param name="newEmail">The new email.</param>
        /// <returns><see cref="CREDENTIAL_CHANGE_SUCCESS"/> if the <see cref="User"/>'s email was updated in the DB,<br/>
        /// <see cref="CREDENTIAL_TAKEN"/> if the email is taken by another account of the same type, and <br/>
        /// <see cref="DB_FAILURE"/> otherwise.</returns>
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

        /// <summary>Updates the given <see cref="User"/>'s password.</summary>
        /// <param name="user"><see cref="User"/> whose password will be updated.</param>
        /// <param name="newPassword"></param>
        /// <returns><see cref="CREDENTIAL_CHANGE_SUCCESS"/> if the <see cref="User"/>'s password was updated in the DB,<br/>
        /// <see cref="DB_FAILURE"/> otherwise.</returns>
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

        /// <summary>Updates the given <see cref="StandardUser"/>'s about.</summary>
        /// <param name="newAbout">New 'about me' text.</param>
        /// <param name="Id">ID of the <see cref="StandardUser"/>.</param>
        /// <returns>True if the user's about was updated. False otherwise.</returns>
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

        /// <summary>Updates the given <see cref="Problem"/>.</summary>
        /// <param name="problem"><see cref="Problem"/> whose information will be updated.</param>
        /// <returns>True if the problem information was successfully updated. False otherwise.</returns>
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

        /// <summary>Retreives all <see cref="AnswerSubmission"/>s for a <see cref="StandardUser"/>.</summary>
        /// <param name="id">ID of the <see cref="StandardUser"/> whose submissions will be retrieved.</param>
        /// <returns>A populated list of submissions, if they exist. An empty list if the user has no submissions. Null if an error occurs.</returns>
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

        /// <summary>Retreives all <see cref="Submission"/>s for an <see cref="Admin"/>.</summary>
        /// <param name="id">ID of the <see cref="Admin"/> whose submissions will be retrieved.</param>
        /// <returns>A populated list of submissions, if they exist. An empty list if the user has no submissions. Null if an error occurs.</returns>
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

        /// <summary>Fetches all <see cref="Problem"/>s from the DB and stores them in memory for easy access.</summary>
        /// <returns>True if all problems were fetched from the DB. False otherwise.</returns>
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

        /// <summary>Inserts a new <see cref="Submission"/> for an <see cref="Admin"/>.</summary>
        /// <param name="submission"><see cref="Submission"/> to insert.</param>
        /// <returns>True if the submission was successfully inserted. False otherwise.</returns>
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

        /// <summary>Retrieves the ranking for a <see cref="StandardUser"/>.</summary>
        /// <param name="userID">ID of the user whose ranking will be retrieved.</param>
        /// <returns>The user's ranking, if it exists and is successfully retrieved. <see cref="DB_FAILURE"/> otherwise.</returns>
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

        /// <summary>Inserts a new <see cref="AnswerSubmission"/> for a <see cref="StandardUser"/>.</summary>
        /// <param name="submission"><see cref="AnswerSubmission"/> to insert.</param>
        /// <param name="currentUserPointsTotal">Current point sum of the <see cref="StandardUser"/> this submission belongs to.</param>
        /// <param name="problemWorth">Point value of the <see cref="Problem"/> this submission belongs to.</param>
        /// <returns>True if the submission was inserted, user ranking and points sum are updated, and problem attempts and completions are updated. False otherwise.</returns>
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

        /// <summary>Retrieves a specific <see cref="Problem"/>.</summary>
        /// <param name="ID">Problem number of the <see cref="Problem"/> to retrieve.</param>
        /// <returns>The desired <see cref="Problem"/>, if it exists and was successfully retrieved. Null otherwise.</returns>
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

        /// <summary>Inserts a new <see cref="StandardUser"/>.</summary>
        /// <param name="user"><see cref="StandardUser"/> to insert.</param>
        /// <returns>The new user's ID, if insertion was successful. <see cref="DB_FAILURE"/> otherwise.</returns>
        public static int InsertNewUser(StandardUser user)
        {
            try
            {
                MySqlCommand command = MakeCommand("INSERT INTO Users(User_Username, User_Password, User_Email, User_Ranking) VALUES(@username, @password, @email, @ranking)");
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@ranking", UserCount+1);
                command.Prepare();
                int result = command.ExecuteNonQuery() == 1 ? (int)command.LastInsertedId : DB_FAILURE;
                connection.Close();
                if (result != DB_FAILURE) UserCount++;
                return result;
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return DB_FAILURE;
        }

        /// <summary>Determines if the given email is associated with a <see cref="User"/>.</summary>
        /// <param name="email">Email to check.</param>
        /// <returns>True if the email is already associated with an account. False otherwise.</returns>
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

        /// <summary>Determines if the given username is associated with a <see cref="User"/>.</summary>
        /// <param name="username">Username to check.</param>
        /// <returns>True if the username is already associated with an account. False otherwise.</returns>
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

        /// <summary>Retrieves the information for a <see cref="StandardUser"/> with the given login credentials.</summary>
        /// <param name="username">Username of the <see cref="StandardUser"/>.</param>
        /// <param name="password">Password of the <see cref="StandardUser"/>.</param>
        /// <returns>The <see cref="StandardUser"/>, fully populated, if one exists with the given credentials. Null otherwise.</returns>
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
                    string fileName = reader.GetString(7);
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
                    result.PictureName = fileName;

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

        /// <summary>Retrieves the information for an <see cref="Admin"/> with the given login credentials.</summary>
        /// <param name="username">Username of the <see cref="Admin"/>.</param>
        /// <param name="password">Password of the <see cref="Admin"/>.</param>
        /// <returns>The <see cref="Admin"/>, fully populated, if one exists with the given credentials. Null otherwise.</returns>
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

        /// <summary>Deletes the <see cref="StandardUser"/> with the given username.</summary>
        /// <param name="username">Username of the <see cref="StandardUser"/>.</param>
        /// <returns>True if:<br/>
        /// - Attempts and completions for all problems the user attempted / completed were updated.<br/>
        /// - User ranking updating occurred. <br/>
        /// - The <see cref="StandardUser"/> and their submission table was deleted.<br/>
        /// False otherwise.
        /// </returns>
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

                if (File.Exists(Program.ImageProjectPath + "/" + user.PictureName + ".png")) File.Delete(Program.ImageProjectPath + "/" + user.PictureName + ".png");

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

        /// <summary>Retrieves a <see cref="StandardUser"/> without a submissions list for account recovery purposes.</summary>
        /// <param name="username">Username of the <see cref="StandardUser"/> to find.</param>
        /// <returns>A reduced <see cref="StandardUser"/> if one exists with the given credential. Null otherwise.</returns>
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
                    string fileName = reader.GetString(7);
                    connection.Close();
                    reader.Close();
                    StandardUser result = new StandardUser(
                        usern, passw, email, rank, points, id, about
                    );
                    result.PictureName = fileName;
                    return result;
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

        /// <summary>Retrieves a <see cref="StandardUser"/> without a submissions list for account recovery purposes.</summary>
        /// <param name="email">Email of the <see cref="StandardUser"/> to find.</param>
        /// <returns>A reduced <see cref="StandardUser"/> if one exists with the given credential. Null otherwise.</returns>
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
                    string fileName = reader.GetString(7);
                    connection.Close();
                    reader.Close();
                    StandardUser result = new StandardUser(
                        usern, passw, email, rank, points, id, about
                    );
                    result.PictureName = fileName;
                    return result;
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

        /// <summary>Currently used exclusively for DB tests. <br/>
        /// Retrieves an <see cref="Admin"/> without a submissions list.</summary>
        /// <param name="username">Username of the <see cref="Admin"/> to find.</param>
        /// <returns>An reduced <see cref="Admin"/> if one exists with the given credential. Null otherwise.</returns>
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

        /// <summary>Currently used exclusively for DB tests. <br/>
        /// Retrieves an <see cref="Admin"/> without a submissions list.</summary>
        /// <param name="email">Email of the <see cref="Admin"/> to find.</param>
        /// <returns>An reduced <see cref="Admin"/> if one exists with the given credential. Null otherwise.</returns>
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

        /// <summary>Retrieves the number of <see cref="StandardUser"/>s in the DB.</summary>
        /// <returns>True if the number of users was successfully retrieved and stored in <see cref="UserCount"/>. False otherwise.</returns>
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

        /// <summary>Retrieves a list of up to 5 <see cref="StandardUser"/>s who have the highest rank in the system.</summary>
        /// <returns>A list of up to 5 <see cref="StandardUser"/>s if users exist in the DB. Null otherwise.</returns>
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
                        user.PictureName = reader.GetString(7);
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

        /// <summary>Retrieves the necessary information to display a <see cref="StandardUser"/>'s profile. </summary>
        /// <param name="username">Username of the <see cref="StandardUser"/> whose profile information is being retrieved.</param>
        /// <returns>A <see cref="StandardUser"/> populated with only the information necessary to generate a profile, if one exists with the given credentials. Null otherwise.</returns>
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
                    user.PictureName = reader.GetString(7);
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

        /// <summary>Retrieves a list of <see cref="StandardUser"/>s whose username is similar to the given query.</summary>
        /// <param name="query">Username or username snippet used to construct list.</param>
        /// <returns>A list of <see cref="StandardUser"/>s that is:<br/> 
        /// - Populated with user information if users with usernames 'like' the query exist. <br/>
        /// - Empty if no users with usernames 'like' the query exist. <br/>
        /// Null otherwise.</returns>
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
                        user.PictureName = reader.GetString(7);
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

        /// <summary>Currently used exclusively for testing. <br/>
        /// Deletes an <see cref="Admin"/>'s <see cref="Submission"/> with the given content.</summary>
        /// <param name="content">Content of the submission to delete.</param>
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

        /// <summary>Currently used exclusively for testing. <br/>
        /// Deletes a <see cref="StandardUser"/>'s <see cref="AnswerSubmission"/> with the given answer.</summary>
        /// <param name="answer">Answer of the <see cref="AnswerSubmission"/> to delete.</param>
        /// <param name="userID">ID of the <see cref="StandardUser"/> whose submission will be deleted.</param>
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

        /// <summary>Current used exclusively for testing. <br/>
        /// Deletes a <see cref="StandardUser"/>'s submission table.</summary>
        /// <param name="tableID">The <see cref="StandardUser"/>'s ID.</param>
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

        /// <summary>Current used exclusively for testing. <br/>
        /// Deletes a <see cref="Problem"/>.</summary>
        /// <param name="problemID">The <see cref="Problem"/>'s problem number.</param>
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

        /// <summary>Retrieves a <see cref="StandardUser"/>'s profile picture file name.</summary>
        /// <param name="Id">The ID of the <see cref="StandardUser"/> whose file name will be retrieved.</param>
        /// <returns>The <see cref="StandardUser"/>'s profile picture file name if it exists and is successfully retrieved. Null otherwise.</returns>
        public static string GetPictureNameByID(int Id) 
        {
            MySqlCommand command = MakeCommand("SELECT User_PictureName FROM Users WHERE User_ID = @id");
            MySqlDataReader reader = null;
            try
            {
                command.Parameters.AddWithValue("@id", Id);
                command.Prepare();
                reader = command.ExecuteReader();
                string result = null;
                if (reader.Read()) result = reader.GetString(0);
                connection.Close();
                reader.Close();
                return result;
            }
            catch
            {
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
                if (reader != null && !reader.IsClosed) reader.Close();
            }
            return null;
        }

        /// <summary>Updates a <see cref="StandardUser"/>'s profile picture file name.</summary>
        /// <param name="Id">The ID of the <see cref="StandardUser"/> whose file name will be updated.</param>
        /// <param name="fileName">The file name of the new profile picture.</param>
        /// <returns>True if the <see cref="StandardUser"/> exists and the file name was successfully updated. False otherwise.</returns>
        public static bool UpdatePictureNameByID(int Id, string fileName)
        {
            try
            {
                MySqlCommand command = MakeCommand("UPDATE Users SET User_PictureName = @fileName WHERE User_ID = @id");
                command.Parameters.AddWithValue("@fileName", fileName);
                command.Parameters.AddWithValue("@id", Id);
                command.Prepare();
                int result = command.ExecuteNonQuery();
                connection.Close();

                // file name added --> one row was affected (the added one)
                return result == 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (connection != null && connection.State == ConnectionState.Open) connection.Close();
            }
            return false;
        }
    }
}
