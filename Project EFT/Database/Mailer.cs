using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Project_EFT.Data_Classes;

namespace Project_EFT.Database
{
    public class Mailer
    {
        private static SmtpClient client;
        private static MailboxAddress noReplyAddress; 


        public static void Init() 
        {
            noReplyAddress = new MailboxAddress("Project EFT", "no-reply-ycpcsp481@gmail.com"); 
            string[] lines = System.IO.File.ReadAllLines("info.txt");
            client = new SmtpClient();
            client.Connect("smtp.gmail.com", 465, true);
            client.Authenticate(lines[4], lines[5]);
        }

        public static bool SendWelcomeEmail(User user)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(noReplyAddress);
                message.To.Add(new MailboxAddress(user.Username, user.Email));
                message.Subject = "Welcome to Project EFT";
                message.Body = new TextPart("plain")
                {
                    Text = String.Format("Welcome, {0}, to Project EFT. There will probably be more text here in the future.", user.Username)
                };
                client.Send(message);
                return true;
            }

            catch (Exception e) 
            {
                Debug.Write(e);
                return false;
            }
        }

        public static bool SendPasswordRecoveryEmail(User user)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(noReplyAddress);
                message.To.Add(new MailboxAddress(user.Username, user.Email));
                message.Subject = "Project EFT - Password Recovery";
                message.Body = new TextPart("plain")
                {
                    Text = String.Format("Hello, {0}. You recently indicated that you forgot your password for your account at Project EFT.\nYour new password is: \n\n{1}\n\nDon't forget to change your password upon logging in.", user.Username, user.Password)
                };
                client.Send(message);
                return true;
            }

            catch (Exception e)
            {
                Debug.Write(e);
                return false;
            }
        }

        public static bool SendAccountRecoveryEmail(User user)
        {
            try
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(noReplyAddress);
                message.To.Add(new MailboxAddress(user.Username, user.Email));
                message.Subject = "Project EFT - Account Recovery";
                message.Body = new TextPart("plain")
                {
                    Text = String.Format("Hello. You recently indicated that you forgot your account information at Project EFT.\nYour username is {0}.\nYour new password is: \n\n{1}\n\nDon't forget to change your password upon logging in.", user.Username, user.Password)
                };
                client.Send(message);
                return true;
            }

            catch (Exception e)
            {
                Debug.Write(e);
                return false;
            }
        }
    }
}
