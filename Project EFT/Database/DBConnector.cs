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
            try
            {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
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
                    return null;
                }
            return null;
        }


        public static bool CheckIfUserSubmissionTableExists(int UserId)
        {
            
            string tablename = "UserSubmissions" + UserId;
            MySqlCommand command = MakeCommand("SELECT * FROM " + tablename);
            try
            {
                command.Prepare();
                MySqlDataReader reader = command.ExecuteReader();
                connection.Close();
                return true;
            }
            catch(MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static bool ResetProblemSubmissions(StandardUser user, int problemID)
        {
            if (CheckIfUserSubmissionTableExists(user.Id))
            {   
                MySqlTransaction transaction = MakeTransaction();
                if (transaction != null) 
                {
                    try
                    {
                        string tablename = "UserSubmissions" + user.Id;
                        MySqlCommand command = new MySqlCommand("SELECT * FROM " + tablename + " WHERE UserSubmissions_ProblemID = @problemNumber", connection, transaction);
                        command.Parameters.AddWithValue("@problemNumber", problemID);
                        command.Prepare();
                        MySqlDataReader reader = command.ExecuteReader();
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
                        transaction.Commit();
                        connection.Close();

                        // refresh problems attempt / completion count the bad way because time crunch
                        GetProblemsList();

                        return true;
                    }
                    catch 
                    {
                        transaction.Rollback();
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }
        public static bool InsertNewProblem(Problem problem)
        {
            try {
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static int TryUpdateUsername(User user, string newUsername)
        {
            try {    
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return DB_FAILURE;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return DB_FAILURE;
            }
        }

        public static int TryUpdateEmail(User user, string newEmail)
        {
            try {
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return DB_FAILURE;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return DB_FAILURE;
            }
        }

        public static int UpdatePassword(User user, string newPassword)
        {
            try {    
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return DB_FAILURE;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return DB_FAILURE;
            }
        }

        public static int UpdateProblem(Problem problem)
        {
            try {    
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return 0;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return 0;
            }
        }

        public static List<AnswerSubmission> GetAnswerSubmissionsByID(int id)
        {
            try {
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static List<Submission> GetAdminSubmissionsByID(int id)
        {
            try {
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static Problem[] GetProblemsList()
        {
            try
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static bool InsertNewAnswerSubmission(AnswerSubmission submission)
        {
            try
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static Problem GetProblemByID(int ID)
        {
            try
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
                return null;
            }
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static int InsertNewUser(StandardUser user)
        {
            try
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return -1;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return -1;
            }
        }

        public static bool DoesEmailExist(string email)
        {
            try
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static bool DoesUsernameExist(string username)
        {
            try
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
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return false;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static StandardUser StandardUserLogin(string username, string password)
        {
            try
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
                    connection.Close();
                    Dictionary<int, List<AnswerSubmission>> submissionMap = new Dictionary<int, List<AnswerSubmission>>();
                    //this will contain all of the users submissions in ascending order, by date, and then add them to the correct lists based on id
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

                    //to check the values of the new submission map, as of right now it worksTM
                    /*foreach(KeyValuePair<int, List<AnswerSubmission>> k in submissionMap)
                    {

                        foreach(AnswerSubmission a in k.Value)
                        {
                            Debug.Write(k.Key + " " + a.SubmissionDate + " " + a.Content + "\n");
                        }
                    }*/
                    return new StandardUser(
                        usern, passw, email, rank, id, submissionMap
                    );
                }
                else
                {
                    connection.Close();
                    return null;
                }
            }
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static Admin AdminLogin(string username, string password)
        {
            try
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
                    return null;
                }
            }
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static bool DeleteUser(string username)
        {
            MySqlCommand command = MakeCommand("SELECT User_ID FROM Users WHERE User_Username = @username");
            try
            {
                command.Parameters.AddWithValue("@username", username);
                command.Prepare();
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int userID = reader.GetInt32(0);
                    connection.Close();
                    MySqlTransaction transaction = null;

                    //if they have a user submissions table, remove their submissions and then drop the table, otherwise just delete the user
                    if (CheckIfUserSubmissionTableExists(userID))
                    {
                        transaction = MakeTransaction();
                        if (transaction != null)
                        {
                            string tablename = "UserSubmissions" + userID;
                            command = new MySqlCommand("SELECT * FROM " + tablename, connection, transaction);
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
                        }
                    }
                    
                    // remove user
                    if (connection.State == ConnectionState.Open) command = new MySqlCommand("DELETE FROM Users WHERE User_ID = @id", connection, transaction);
                    else if (OpenConnection()) command = new MySqlCommand("DELETE FROM Users WHERE User_ID = @id", connection);
                    else throw new Exception("Could not open new connection when deleting user");
                    command.Parameters.AddWithValue("@id", userID);
                    command.Prepare();
                    int result = command.ExecuteNonQuery();
                    if (transaction != null) try { transaction.Commit(); } catch { transaction.Rollback(); }
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

            catch (MySqlException e)
            {
                Debug.WriteLine(e);
                if (connection.State == ConnectionState.Open) connection.Close();
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
            }
            return false;
        }

        public static StandardUser GetStandardUserByUsername(string username) 
        {
            try
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
                    return null;
                }
            }
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

        public static StandardUser GetStandardUserByEmail(string email)
        {
            try
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
                    return null;
                }
            }
            catch (MySqlException e)
            {
                connection.Close();
                Debug.Write(e);
                return null;
            }
            catch (NullReferenceException e)
            {
                Debug.Write(e);
                return null;
            }
        }

    }
}
