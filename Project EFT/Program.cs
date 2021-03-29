using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Project_EFT.Database;
using MailKit.Net.Smtp;
using MimeKit;
using System.Diagnostics;
using System.Reflection;
using Project_EFT.Ciphers;
using Project_EFT.Ciphers.Options;
using Newtonsoft.Json;

namespace Project_EFT
{
    public class Program
    {

        private const string ciphersNameSpace = "Project_EFT.Ciphers";
        public static List<Type> CipherList;
        public static JsonSerializerSettings DerivedJSONSettings;
        public static List<string> CipherNames;

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
