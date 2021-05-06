using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Project_EFT.Database;
using System.Reflection;
using Project_EFT.Ciphers;
using Newtonsoft.Json;

namespace Project_EFT
{
    public class Program
    {

        private const string ciphersNameSpace = "Project_EFT.Ciphers";
        public static List<Type> CipherList;
        public static JsonSerializerSettings DerivedJSONSettings;
        public static List<string> CipherNames;
        /// <summary>Path to the user profile pictures folder (from an HTML perspective) where the root folder is wwwroot.</summary>
        public const string ImageWebPath = "/Images";

        /// <summary>Path to the user profile pictures folder (from a Solution perspective) where the root folder is Project EFT.</summary>
        public const string ImageProjectPath = "wwwroot/Images";

        public static void Main(string[] args)
        {
            // connect to database
            DBConnector.Init();

            // connect to mail server
            Mailer.Init();

            CipherList = new List<Type>();
            CipherNames = new List<string>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == ciphersNameSpace)
                {
                    if (type.Name != "Cipher") 
                    {
                        CipherList.Add(type);
                        CipherNames.Add(((Cipher)Activator.CreateInstance(type)).Name);
                    }
                }
            }

            DerivedJSONSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ObjectCreationHandling = ObjectCreationHandling.Replace
                // PreserveReferencesHandling = PreserveReferencesHandling.All
            };

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
