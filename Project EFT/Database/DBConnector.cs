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

        public const int DB_FAILURE = -1;
        public const int CREDENTIAL_CHANGE_SUCCESS = 100;
        public const int CREDENTIAL_TAKEN = 101;

        public static void Init()
        {
            string[] lines = System.IO.File.ReadAllLines("../../info.txt");
            connectionString = @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3];
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

        public static bool InsertNewProblem(Problem problem)
        {
            MySqlCommand command = MakeCommand("INSERT INTO Problems (Problem_Title, Problem_Question, Problem_Answer) VALUES (@title, @question, @answer)");
            command.Parameters.AddWithValue("@title", problem.Title);
            command.Parameters.AddWithValue("@question", problem.Question);
            command.Parameters.AddWithValue("@answer", problem.Answer);
            command.Prepare();
            int numRowsAffected = command.ExecuteNonQuery();
            connection.Close();
            
            // problem added --> one row was affected (the added one)
            return numRowsAffected == 1;
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

            return result;
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
            MySqlCommand command = MakeCommand("SELECT * FROM Problems");
            MySqlDataReader reader = command.ExecuteReader();
            List<Problem> problems = new List<Problem>();
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
            MySqlCommand command = MakeCommand("INSERT INTO AnswerSubmissions(User_ID, AnswerSubmissions_Answer, AnswerSubmissions_SubmissionDate, AnswerSubmissions_IsCorrect, AnswerSubmissions_ProblemID) VALUES(@user_id, @content, @date, @correct, @problem_id)");
            command.Parameters.AddWithValue("@user_id", submission.UserID);
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

        public static bool GetProblemCorrectValueByUserAndProblemID(int userID, int problemID)
        {
            MySqlCommand command = MakeCommand("SELECT AnswerSubmissions_IsCorrect FROM AnswerSubmissions WHERE User_ID = @user_id AND AnswerSubmissions_ProblemID = @problem_id AND AnswerSubmissions_IsCorrect = TRUE");
            command.Parameters.AddWithValue("@user_id", userID);
            command.Parameters.AddWithValue("@problem_id", problemID);
            command.Prepare();
            MySqlDataReader reader = command.ExecuteReader();
            bool result = reader.Read();
            
            connection.Close();
            
            return result;
        }

        public static bool InsertNewUser(StandardUser user)
        {
            MySqlCommand command = MakeCommand("INSERT INTO Users(User_Username, User_Password, User_Email) VALUES(@username, @password, @email)");
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Prepare();
            int result = command.ExecuteNonQuery();
            connection.Close();

            // user added --> one row was affected (the added one)
            return result == 1;
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
                // TODO must get submissions for user before returning
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
            else {
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
            MySqlCommand command = MakeCommand("DELETE FROM Users WHERE User_Username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Prepare();
            int result = command.ExecuteNonQuery();
            connection.Close();

            // user deleted --> one row was affected (the deleted one)
            return result == 1;
        }
    }
}
