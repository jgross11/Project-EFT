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

namespace Project_EFT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // connect to database
            DBConnector.Init();

            // connect to mail server
            Mailer.Init();

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
