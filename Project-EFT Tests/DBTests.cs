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
        string userEmail, fakeUserEmail, updateEmail, username, fakeUsername, updateUsername;
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
            updateEmail = "youCantMakeThisEmail";

            username = "doNotDelete";
            fakeUsername = "thisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRightthisOneHasToBePastTheUserNameCharacterLimitsRight";
            updateUsername = "thisWontWorkIfSomeonePickedThisUsername";

            Dictionary<int, List<AnswerSubmission>> userSubMap = new Dictionary<int, List<AnswerSubmission>>();
            userSubMap.Add(1, new List<AnswerSubmission>() { sub1 });
            userSubMap.Add(2, new List<AnswerSubmission>() { sub2 });

            user = new StandardUser(username, "49e5d4e3749b2f33b75dddc984878cdc", userEmail, -1, 1, 80, userSubMap);
            user.About = "Dont Delete This About Me";
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
            bool nextTestRun;

            //checks the size of the expected list and the returned list
            Assert.True(nextTestRun = testResults.Count == usersSubmissions.Count);

            //if the above test fails, this test would throw an error (index out of bounds) so it is not run
            if (nextTestRun) {
                //iterates through the specific list and checks to see if they equal the expected values set in the setup function
                int counter = 0;
                foreach (AnswerSubmission sub in testResults)
                {
                    Assert.True(sub.IsEqual(usersSubmissions[counter]));
                    counter++;
                }
            }
        }

        [Test]
        public void testGetAdminSubmissionsByID()
        {

            //gets the submission list for the test user
            List<Submission> testResults = DBConnector.GetAdminSubmissionsByID(adminID);
            bool nextTestRun;

            Assert.True(nextTestRun = testResults.Count == adminSubmissions.Count);
            //if the above test fails, this test would throw an error (index out of bounds) so it is not run
            if (nextTestRun) {
                //iterates through the specific list and checks to see if they equal the expected values set in the setup function
                int counter = 0;
                foreach (Submission sub in testResults)
                {
                    Assert.True(sub.IsEqual(adminSubmissions[counter]));
                    counter++;
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
            Assert.IsNull(DBConnector.GetStandardUserByEmail(fakeUserEmail));
        }

        [Test]
        public void testGetAdminByUsername()
        {
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).IsEqual(admin));
            Assert.IsNull(DBConnector.GetAdminByUsername(fakeUsername));
        }

        [Test]
        public void testGetAdminByEmail()
        {
            Assert.True(DBConnector.GetAdminByEmail(admin.Email).IsEqual(admin));
            Assert.IsNull(DBConnector.GetAdminByEmail(fakeUserEmail));
        }

        [Test]
        public void testGetUserProfileInformationByUsername()
        {
            Assert.True(DBConnector.GetUserProfileInformationByUsername(user.Username).IsEqualForProfile(user));
            Assert.IsNull(DBConnector.GetUserProfileInformationByUsername(fakeUsername));
        }
        


        //updates
        [Test]
        public void testTryUpdateUsername()
        {
            String resetUsername = user.Username;
            //runs the initial update query, and checks to see if it was updated to the expected username change
            DBConnector.TryUpdateUsername(user, updateUsername);
            Assert.True(DBConnector.GetStandardUserByEmail(user.Email).Username.Equals(updateUsername));

            //sets the username back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateUsername(user, resetUsername);
            Assert.True(DBConnector.GetStandardUserByEmail(user.Email).Username.Equals(resetUsername));

            String resetAdminUsername = admin.Username;
            //runs the initial update query, and checks to see if it was updated to the expected username change
            DBConnector.TryUpdateUsername(admin, updateUsername);
            Assert.True(DBConnector.GetAdminByEmail(admin.Email).Username.Equals(updateUsername));

            //sets the username back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateUsername(admin, resetAdminUsername);
            Assert.True(DBConnector.GetAdminByEmail(admin.Email).Username.Equals(resetAdminUsername));

        }

        [Test]
        public void testTryUpdateEmail()
        {
            String resetEmail = user.Email;
            //runs the initial update query, and checks to see if it was updated to the expected username change
            DBConnector.TryUpdateEmail(user, updateEmail);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Email.Equals(updateEmail));

            //sets the username back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateEmail(user, resetEmail);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Email.Equals(resetEmail));

            String resetAdminEmail = admin.Email;
            //runs the initial update query, and checks to see if it was updated to the expected username change
            DBConnector.TryUpdateEmail(admin, updateEmail);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Email.Equals(updateEmail));

            //sets the username back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateEmail(admin, resetAdminEmail);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Email.Equals(resetAdminEmail));
        }





        //TODO inserts/deletes

    }
}
