using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Project_EFT.Data_Classes;

namespace Project_EFT_Tests
{
    class InformationValidatorTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestEmailValidation()
        {
            Assert.False(InformationValidator.VerifyInformation("", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation(null, InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("7", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc.", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@a", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@a.", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@.", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@.a", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@.@", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@..", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@@..", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("                                      abc@@..", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@                                      @..", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@@                                      ..", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@@.                                      .", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("abc@@..                                      ", InformationValidator.EmailType));
            Assert.False(InformationValidator.VerifyInformation("validformat@butitsway.toolongtostoreindatabase", InformationValidator.EmailType));

            Assert.True(InformationValidator.VerifyInformation("a@b.c", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("jgross11@ycp.edu", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("                                                                         jgross11@ycp.edu", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("jgross11@ycp.edu                                                                         ", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("                                         jgross11@ycp.edu                                ", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("josh.gross@ycp.edu", InformationValidator.EmailType));
            Assert.True(InformationValidator.VerifyInformation("josh.gross@york.college.edu", InformationValidator.EmailType));
        }

        [Test]
        public void TestPasswordValidation()
        {
            Assert.False(InformationValidator.VerifyInformation("", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation(null, InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("abcdefg", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("ABCDEFG", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("test", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("abcdef!", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("abcdef!", InformationValidator.PasswordType));
            Assert.False(InformationValidator.VerifyInformation("abcdefABCDEF0123456789abcdefABCDEF0123456789", InformationValidator.PasswordType));

            Assert.True(InformationValidator.VerifyInformation("abcdef", InformationValidator.PasswordType));
            Assert.True(InformationValidator.VerifyInformation("ABCDEF", InformationValidator.PasswordType));
            Assert.True(InformationValidator.VerifyInformation("0123456789", InformationValidator.PasswordType));
            Assert.True(InformationValidator.VerifyInformation("012abcDEF", InformationValidator.PasswordType));
            Assert.True(InformationValidator.VerifyInformation("ABCDEF", InformationValidator.PasswordType));
        }

        [Test]
        public void TestUsernameValidation()
        {
            Assert.False(InformationValidator.VerifyInformation("", InformationValidator.UsernameType));
            Assert.False(InformationValidator.VerifyInformation(null, InformationValidator.UsernameType));
            Assert.False(InformationValidator.VerifyInformation("                                             ", InformationValidator.UsernameType));
            Assert.False(InformationValidator.VerifyInformation("thisusernameisdefinitelytoolongtostoreinthedatabase", InformationValidator.UsernameType));

            Assert.True(InformationValidator.VerifyInformation("a", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("                                                                                a", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("a                                                                                ", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("                                         a                                       ", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("abc", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("abcdefg", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjk", InformationValidator.UsernameType));
            Assert.True(InformationValidator.VerifyInformation("qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjk", InformationValidator.UsernameType));
        }
        [Test]
        public void TestProblemSubmissionValidation()
        {
            Assert.False(InformationValidator.VerifyInformation("", InformationValidator.ProblemSubmissionType));
            Assert.False(InformationValidator.VerifyInformation(null, InformationValidator.ProblemSubmissionType));
            Assert.False(InformationValidator.VerifyInformation("                                             ", InformationValidator.ProblemSubmissionType));
            Assert.False(InformationValidator.VerifyInformation(@"thissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
thissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabasethissubmissionisdefinitelytoolongtostoreinthedatabase
", InformationValidator.ProblemSubmissionType));

            Assert.True(InformationValidator.VerifyInformation("a", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("                                                                                a", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("a                                                                                ", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("                                         a                                       ", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("abc", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("abcdefg", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation("qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjk", InformationValidator.ProblemSubmissionType));
            Assert.True(InformationValidator.VerifyInformation(@"qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjk
qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjkqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjkqwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjk
", InformationValidator.ProblemSubmissionType));
        }

        [Test]
        public void TestProblemTitleValidation()
        {
            Assert.False(InformationValidator.VerifyInformation("", InformationValidator.ProblemTitleType));
            Assert.False(InformationValidator.VerifyInformation(null, InformationValidator.ProblemTitleType));
            Assert.False(InformationValidator.VerifyInformation("         ", InformationValidator.ProblemTitleType));
            Assert.False(InformationValidator.VerifyInformation("thistitleistoolongtofitinthedatabasesowemustdenyit", InformationValidator.ProblemTitleType));

            Assert.True(InformationValidator.VerifyInformation("a valid title", InformationValidator.ProblemTitleType));
            Assert.True(InformationValidator.VerifyInformation("                                                                     a valid title", InformationValidator.ProblemTitleType));
            Assert.True(InformationValidator.VerifyInformation("a valid title                                                                      ", InformationValidator.ProblemTitleType));
            Assert.True(InformationValidator.VerifyInformation("                                      a valid title                                ", InformationValidator.ProblemTitleType));
        }

        [Test]
        public void TestTemporaryPasswordGeneration()
        {
            int testQuantity = 100000;
            int threshold = 10;
            string[] generatedPasswords = new string[testQuantity];
            for (int i = 0; i < testQuantity; i++)
            {
                generatedPasswords[i] = InformationValidator.GenerateTemporaryPassword();
            }
            HashSet<string> passwordCounts = new HashSet<string>();
            int numRepeats = 0;
            foreach (string pass in generatedPasswords) {
                Assert.True(pass.Length == 16);
                int lowers = 4;
                int uppers = 4;
                int numbers = 4;
                int symbols = 4;
                foreach (char c in pass)
                {
                    if ("qwertyuiopasdfghjklzxcvbnm".Contains(c)) lowers--;
                    if ("MNBVCXZQWERTYUIOPHJKLFDSAG".Contains(c)) uppers--;
                    if ("0512864973".Contains(c)) numbers--;
                    if (">/?=*@[}^&+.$\\|]~-()`#!_%".Contains(c)) symbols--;
                }

                Assert.True(lowers == 0);
                Assert.True(uppers == 0);
                Assert.True(numbers == 0);
                Assert.True(symbols == 0);

                if (passwordCounts.Contains(pass))
                {
                    numRepeats++;
                }
                else {
                    passwordCounts.Add(pass);
                }
            }

            Assert.True(numRepeats < threshold);
        }

        [Test]
        public void TestMD5Hash() 
        {
            int testQuantity = 100000;
            string[] plains = new string[testQuantity];
            string[] hashes = new string[testQuantity];
            for (int i = 0; i < testQuantity; i++) 
            {
                plains[i] = InformationValidator.GenerateTemporaryPassword();
            }
            HashSet<string> plainCounts = new HashSet<string>();
            int repeats = 0;
            foreach (string plain in plains) 
            {
                if (plainCounts.Contains(plain))
                {
                    repeats++;
                }
                else 
                {
                    plainCounts.Add(plain);
                }
            }
            for (int i = 0; i < testQuantity; i++) 
            {
                hashes[i] = InformationValidator.MD5Hash(plains[i]);
                Assert.True(InformationValidator.VerifyInformation(hashes[i], InformationValidator.PasswordType));
            }
            HashSet<string> hashCounts = new HashSet<string>();
            int collisions = 0;
            foreach (string hash in hashes) 
            {
                if (hashCounts.Contains(hash))
                {
                    collisions++;
                }
                else 
                {
                    hashCounts.Add(hash);
                }
            }
            Assert.True(collisions == repeats);
        }
    }
}