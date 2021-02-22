using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Project_EFT.Database
{
    public class DBConnector
    {
        public static MySqlConnection connection;
        
        public static bool Init() 
        {
            string[] lines = System.IO.File.ReadAllLines("../../info.txt");
            
            // dependent on the structure of the file...
            connection = new MySqlConnection(
                @"server=" + lines[0] + ";userid=" + lines[1] + ";password=" + lines[2] + ";database=" + lines[3]
            );
            connection.Open();
            return true;
        }


    }
}
