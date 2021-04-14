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
    }
}