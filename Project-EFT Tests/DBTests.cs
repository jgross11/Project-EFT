using NUnit.Framework;
using Project_EFT.Data_Classes;
using Project_EFT.Ciphers;
using Project_EFT.Ciphers.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using Project_EFT.Database;


namespace Project_EFT_Tests
{
    class DBTest
    {

        //these tests will be using a specific user, "doNotDelete" to run the tests on.  For now I have decided not to test any update/insert/delete queries, as 
        //we currently do not have a verifiable way to check the number of connections to the database, so as to prevent dependencies and irregular performance for our "users"
        //i.e. they could "see" the test problem inserted into the database, since we can not see if there are no users connected

        StandardUser user;
        Admin admin;
        int userID, fakeUserID, adminID;
        string userEmail, fakeUserEmail, username, fakeUsername;
        AnswerSubmission sub1, sub2;
        List<AnswerSubmission> usersSubmissions;
        List<Submission> adminSubmissions;
        Problem problem;

        [SetUp]
        public void Setup()
        {

            userID = 80;
            fakeUserID = 0;
            adminID = 2;


            sub1 = new AnswerSubmission("you've solved the first problem", new DateTime(2021, 4, 21, 17, 40, 23), userID, false, 1);
            sub2 = new AnswerSubmission("Pdwk dqg FV duh frro!", new DateTime(2021, 4, 21, 17, 40, 32), userID, true, 2);

            usersSubmissions = new List<AnswerSubmission>();
            usersSubmissions.Add(sub1);
            usersSubmissions.Add(sub2);

            adminSubmissions = new List<Submission>();
            adminSubmissions.Add(new Submission("Deleted account with username: new19", new DateTime(2021, 4, 21, 19, 52, 37), 2));

            problem = new Problem(1, "A Simple Caesar", "Decrypt the following Caesar cipher: brx'yh vroyhg wkh iluvw sureohp!", "you've solved the first problem!", -1, -1, 1);


            userEmail = "doNotDelete@doNotDelete.com";
            fakeUserEmail = "whyWouldAnyoneMakeThis@anEmail.com";

            username = "doNotDelete";
            fakeUsername = "thisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRight";

            Dictionary<int, List<AnswerSubmission>> userSubMap = new Dictionary<int, List<AnswerSubmission>>();
            userSubMap.Add(1, new List<AnswerSubmission>() { sub1 });
            userSubMap.Add(2, new List<AnswerSubmission>() { sub2 });

            user = new StandardUser(username, "49e5d4e3749b2f33b75dddc984878cdc", userEmail, -1, -1, 80, userSubMap);

            admin = new Admin("doNotDeleteAdmin", "7c008e2d5c3a4e283dd97ad218534ee7", "doNotDeleteAdmin@doNotDelete.com", adminID);


            DBConnector.InitForTests();

        }

        


        //gets/checks
        [Test]
        public void testCheckIfUserSubmissionTableExists()
        {
            Assert.True(DBConnector.CheckIfUserSubmissionTableExists(userID));

            Assert.False(DBConnector.CheckIfUserSubmissionTableExists(fakeUserID));
        }

        [Test]
        public void testGetAnswerSubmissionsByID()
        {
            //gets the submission list for the test user
            List<AnswerSubmission> testResults = DBConnector.GetAnswerSubmissionsByID(userID);

            //iterates through the specific list and checks to see if they equal the expected values set in the setup function
            int counter = 0;
            foreach(AnswerSubmission sub in testResults)
            {
                Assert.True(sub.IsEqual(usersSubmissions[counter]));
                counter++;

                if (counter > usersSubmissions.Count)
                {
                    break;
                }
            }
        }

        [Test]
        public void testGetAdminSubmissionsByID()
        {

            //gets the submission list for the test user
            List<Submission> testResults = DBConnector.GetAdminSubmissionsByID(adminID);

            //iterates through the specific list and checks to see if they equal the expected values set in the setup function
            int counter = 0;
            foreach (Submission sub in testResults)
            {
                Assert.True(sub.IsEqual(adminSubmissions[counter]));
                counter++;

                if (counter > adminSubmissions.Count)
                {
                    break;
                }
            }

        }

        [Test]
        public void testGetProblemByID()
        {
            Assert.True(DBConnector.GetProblemByID(problem.ProblemNumber).IsEqual(problem));
            Assert.IsNull(DBConnector.GetProblemByID(-1));
        }

        [Test]
        public void testDoesEmailExist()
        {
            Assert.True(DBConnector.DoesEmailExist(userEmail));
            Assert.False(DBConnector.DoesEmailExist(fakeUserEmail));
        }

        [Test]
        public void testDoesUsernameExist()
        {
            Assert.True(DBConnector.DoesUsernameExist(username));
            Assert.False(DBConnector.DoesUsernameExist(fakeUsername));
        }

        [Test]
        public void testStandardUserLogin()
        {
            Assert.True(DBConnector.StandardUserLogin(user.Username, user.Password).IsEqualWithSubMap(user));
            Assert.IsNull(DBConnector.StandardUserLogin(fakeUsername, "thisDoesntMatter"));
        }

        [Test]
        public void testAdminLogin()
        {
            Assert.True(DBConnector.AdminLogin(admin.Username, admin.Password).IsEqual(admin));
            Assert.IsNull(DBConnector.AdminLogin(fakeUsername, "thisDoesntMatter"));
        }

        [Test]
        public void testGetStandardUserByUsername()
        {
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).IsEqual(user));
            Assert.IsNull(DBConnector.GetStandardUserByUsername(fakeUsername));
        }

        [Test]
        public void testGetStandardUserByEmail()
        {
            Assert.True(DBConnector.GetStandardUserByEmail(user.Email).IsEqual(user));
            Assert.IsNull(DBConnector.GetStandardUserByUsername(fakeUserEmail));
        }

        

        //TODO Possibly inserts/deletes/updates

    }
}
