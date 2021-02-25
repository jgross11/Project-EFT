using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Project_EFT.Data_Classes;

namespace Project_EFT.Database
{
    public class DBConnector
    {
        public static MySqlConnection connection;
        public static string connectionString;

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
            command.Parameters.AddWithValue("@title", problem.title);
            command.Parameters.AddWithValue("@question", problem.question);
            command.Parameters.AddWithValue("@answer", problem.answer);
            command.Prepare();
            command.ExecuteNonQuery();
            connection.Close();
            return true;
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

        public static Problem GetProblemByID(int ID)
        {


            MySqlCommand command = MakeCommand("SELECT * FROM Problems WHERE Problem_Number = @id");
            command.Parameters.AddWithValue("@id", ID);
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

    }
}
