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

        StandardUser user, fakeUser;
        Admin admin;
        int userID, fakeUserID, adminID;
        string userEmail, fakeUserEmail, updateEmail, username, fakeUsername, updateUsername, updatePassword, updateAbout;
        AnswerSubmission sub1, sub2;
        List<AnswerSubmission> usersSubmissions;
        List<Submission> adminSubmissions;
        Problem problem, updateProblem;

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
            updateProblem = new Problem(1, "This should be different", "this too", "the answer?", 5, 5, 2);

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

            updatePassword = "thisWouldBeHashed";

            updateAbout = "thisShouldBeReset";

            fakeUser = new StandardUser(updateUsername, updatePassword, updateEmail);

            DBConnector.InitForTests();

        }

        
        //gets/checks
        [Test]
        public void TestCheckIfUserSubmissionTableExists()
        {
            Assert.True(DBConnector.CheckIfUserSubmissionTableExists(userID));

            Assert.False(DBConnector.CheckIfUserSubmissionTableExists(fakeUserID));
        }

        [Test]
        public void TestGetAnswerSubmissionsByID()
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
        public void TestGetAdminSubmissionsByID()
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
        public void TestGetProblemByID()
        {
            Assert.True(DBConnector.GetProblemByID(problem.ProblemNumber).IsEqual(problem));
            Assert.IsNull(DBConnector.GetProblemByID(-1));
        }

        [Test]
        public void TestDoesEmailExist()
        {
            Assert.True(DBConnector.DoesEmailExist(userEmail));
            Assert.False(DBConnector.DoesEmailExist(fakeUserEmail));
        }

        [Test]
        public void TestDoesUsernameExist()
        {
            Assert.True(DBConnector.DoesUsernameExist(username));
            Assert.False(DBConnector.DoesUsernameExist(fakeUsername));
        }

        [Test]
        public void TestStandardUserLogin()
        {
            Assert.True(DBConnector.StandardUserLogin(user.Username, user.Password).IsEqualWithSubMap(user));
            Assert.IsNull(DBConnector.StandardUserLogin(fakeUsername, "thisDoesntMatter"));
        }

        [Test]
        public void TestAdminLogin()
        {
            Assert.True(DBConnector.AdminLogin(admin.Username, admin.Password).IsEqual(admin));
            Assert.IsNull(DBConnector.AdminLogin(fakeUsername, "thisDoesntMatter"));
        }

        [Test]
        public void TestGetStandardUserByUsername()
        {
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).IsEqual(user));
            Assert.IsNull(DBConnector.GetStandardUserByUsername(fakeUsername));
        }

        [Test]
        public void TestGetStandardUserByEmail()
        {
            Assert.True(DBConnector.GetStandardUserByEmail(user.Email).IsEqual(user));
            Assert.IsNull(DBConnector.GetStandardUserByEmail(fakeUserEmail));
        }

        [Test]
        public void TestGetAdminByUsername()
        {
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).IsEqual(admin));
            Assert.IsNull(DBConnector.GetAdminByUsername(fakeUsername));
        }

        [Test]
        public void TestGetAdminByEmail()
        {
            Assert.True(DBConnector.GetAdminByEmail(admin.Email).IsEqual(admin));
            Assert.IsNull(DBConnector.GetAdminByEmail(fakeUserEmail));
        }

        [Test]
        public void TestGetUserProfileInformationByUsername()
        {
            Assert.True(DBConnector.GetUserProfileInformationByUsername(user.Username).IsEqualForProfile(user));
            Assert.IsNull(DBConnector.GetUserProfileInformationByUsername(fakeUsername));
        }

        [Test]
        public void TestGetUserRanking()
        {
            //Makes sure the dependent query works, if this fails, the actual tests wont run
            TestGetStandardUserByUsername(); 

            //get the real ranking of the test user, from a verified method, then testing the actual method
            int testUserRank = DBConnector.GetStandardUserByUsername(user.Username).Ranking;
            Assert.True(DBConnector.GetUserRanking(user.Id) == testUserRank);

            //tests to make sure an incorrect ID gets the expected value
            Assert.True(DBConnector.GetUserRanking(fakeUserID) == DBConnector.DB_FAILURE);
        }


        //updates
        [Test]
        public void TestTryUpdateUsername()
        {
            //Makes sure the dependent queries works, if this fails, the actual tests wont run
            TestGetStandardUserByEmail();
            TestGetAdminByEmail();

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
        public void TestTryUpdateEmail()
        {
            //Makes sure the dependent queries works, if this fails, the actual tests wont run
            TestGetStandardUserByUsername();
            TestGetAdminByUsername();

            String resetEmail = user.Email;
            //runs the initial update query, and checks to see if it was updated to the expected email change
            DBConnector.TryUpdateEmail(user, updateEmail);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Email.Equals(updateEmail));

            //sets the email back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateEmail(user, resetEmail);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Email.Equals(resetEmail));

            String resetAdminEmail = admin.Email;
            //runs the initial update query, and checks to see if it was updated to the expected email change
            DBConnector.TryUpdateEmail(admin, updateEmail);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Email.Equals(updateEmail));

            //sets the email back to its correct value, so as to work for the future and other tests
            DBConnector.TryUpdateEmail(admin, resetAdminEmail);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Email.Equals(resetAdminEmail));
        }

        [Test]
        public void TestUpdatePassword()
        {
            //Makes sure the dependent queries works, if this fails, the actual tests wont run
            TestGetStandardUserByUsername();
            TestGetAdminByUsername();

            String resetPassword = user.Password;
            //runs the initial update query, and checks to see if it was updated to the expected password change
            DBConnector.UpdatePassword(user, updatePassword);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Password.Equals(updatePassword));

            //sets the password back to its correct value, so as to work for the future and other tests
            DBConnector.UpdatePassword(user, resetPassword);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).Password.Equals(resetPassword));

            String resetAdminPassword = admin.Password;
            //runs the initial update query, and checks to see if it was updated to the expected password change
            DBConnector.UpdatePassword(admin, updatePassword);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Password.Equals(updatePassword));

            //sets the password back to its correct value, so as to work for the future and other tests
            DBConnector.UpdatePassword(admin, resetAdminPassword);
            Assert.True(DBConnector.GetAdminByUsername(admin.Username).Password.Equals(resetAdminPassword));
        }

        [Test]
        public void TestUpdateAbout()
        {
            //Makes sure the dependent query works, if this fails, the actual tests wont run
            TestGetStandardUserByUsername();
            
            String resetAbout = user.About;
            //runs the initial update query, and checks to see if it was updated to the expected "about" change
            DBConnector.UpdateAbout(updateAbout, user.Id);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).About.Equals(updateAbout));

            //sets the "about" back to its correct value, so as to work for the future and other tests
            DBConnector.UpdateAbout(resetAbout, user.Id);
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).About.Equals(resetAbout));
        }

        [Test]
        public void TestUpdateProblem()
        {
            //Makes sure the dependent query works, if this fails, the actual tests wont run
            TestGetProblemByID();

            //store the original problem so as to revert all changes back to normal
            Problem resetProblem = DBConnector.GetProblemByID(updateProblem.ProblemNumber);

            //update to the "new" problem values
            DBConnector.UpdateProblem(updateProblem);

            Assert.True(DBConnector.GetProblemByID(updateProblem.ProblemNumber).IsEqual(updateProblem));

            //change the values back
            DBConnector.UpdateProblem(resetProblem);

            Assert.True(DBConnector.GetProblemByID(updateProblem.ProblemNumber).IsEqual(resetProblem));
        }

        //inserts
        [Test]
        public void TestInsertNewProblem()
        {
            //test query dependencies
            TestGetProblemByID();
            int newProblemId = DBConnector.problems.Count + 1;
            Problem newProblem = new Problem(newProblemId, "this should be deleted", "is it?", "I hope so", 0, 0, 1);

            //assure problem is not already there
            Assert.IsNull(DBConnector.GetProblemByID(newProblemId));

            //Insert new problem
            DBConnector.InsertNewProblem(newProblem);

            //make sure it is there
            Assert.True(DBConnector.GetProblemByID(newProblemId).IsEqual(newProblem));

            //delete it
            DBConnector.DeleteProblemByID(newProblemId);

            //assure problem is gone
            Assert.IsNull(DBConnector.GetProblemByID(newProblemId));
        }

        [Test]
        public void TestInsertNewAdminSubmission()
        {
            //Makes sure the dependent query works, if this fails, the actual tests wont run
            TestGetAdminSubmissionsByID();

            //this is disgusting, but if you just send in dateTime.Now, *technically* the date returned by the DB is earlier than the actual date time, since
            //the dateTime we have here would have milliseconds as well....took me some time to figure out why it wasnt passing
            DateTime dateTime = DateTime.Now;
            DateTime actualDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            Submission deleteSubmission = new Submission("This should be deleted", actualDateTime, admin.Id);

            //insert the new admin submission, and check if it is the most recent submission in the admins submissionList
            DBConnector.InsertNewAdminSubmission(deleteSubmission);
            List<Submission> testSubList = DBConnector.GetAdminSubmissionsByID(admin.Id);
            Assert.True(testSubList[^1].IsEqual(deleteSubmission));

            //deletes the inserted submission so that the table is reset to how it should be
            DBConnector.DeleteAdminSubmissionByContent(deleteSubmission.Content);
            List<Submission> originalSubList = DBConnector.GetAdminSubmissionsByID(admin.Id);
            Assert.True(originalSubList.Count == testSubList.Count - 1);
        }

        [Test]
        public void TestInsertNewAnswerSubmission()
        {
            //Makes sure the dependent queries works, if this fails, the actual tests wont run
            TestGetAnswerSubmissionsByID();
            TestUpdateProblem();
            TestGetProblemByID();

            //getting the problem used in this test
            Problem problem = DBConnector.GetProblemByID(3);

            //this is disgusting, but if you just send in dateTime.Now, *technically* the date returned by the DB is earlier than the actual date time, since
            //the dateTime we have here would have milliseconds as well....took me some time to figure out why it wasnt passing
            DateTime dateTime = DateTime.Now;
            DateTime actualDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            AnswerSubmission deleteSubmission = new AnswerSubmission("This should be deleted", actualDateTime, user.Id, false, problem.ProblemNumber);

            //insert the new answer submission, and check if it is the most recent submission in the admins submissionList
            DBConnector.InsertNewAnswerSubmission(deleteSubmission, user.PointsTotal, 1);
            List<AnswerSubmission> testSubList = DBConnector.GetAnswerSubmissionsByID(user.Id);
            Assert.True(testSubList[^1].IsEqual(deleteSubmission));

            //checks if newProblem values is same as old + 1 attempt
            Problem newProblem = DBConnector.GetProblemByID(problem.ProblemNumber);
            Assert.True(newProblem.IsEqual(new Problem(problem.ProblemNumber, problem.Title, problem.Question, problem.Answer, problem.Attempts + 1, problem.Completions, problem.PointsValue)));

            //deletes the inserted submission so that the table is reset to how it should be
            DBConnector.DeleteAnswerSubmissionByAnswer(deleteSubmission.Content, user.Id);
            List<AnswerSubmission> originalSubList = DBConnector.GetAnswerSubmissionsByID(user.Id);
            Assert.True(originalSubList.Count == testSubList.Count - 1);

            //resets the problem values back
            DBConnector.UpdateProblem(problem);
            Assert.True(DBConnector.GetProblemByID(problem.ProblemNumber).IsEqual(problem));
        }

        [Test]
        public void TestInsertNewUser()
        {
            //test query dependencies
            TestGetStandardUserByUsername();

            //assure user does not already exist
            Assert.IsNull(DBConnector.GetStandardUserByUsername(fakeUser.Username));

            //insert new user
            DBConnector.InsertNewUser(fakeUser);

            //make sure user is there
            Assert.True(DBConnector.GetStandardUserByUsername(fakeUser.Username).IsEqualWithOnlyUsernameEmailPassword(fakeUser));

            //delete the user
            DBConnector.DeleteUser(fakeUser.Username);

            //assure user is out of the database
            Assert.IsNull(DBConnector.GetStandardUserByUsername(fakeUser.Username));
        }


        //reset
        [Test]
        public void TestResetProblemSubmissions()
        {
            //test query dependencies
            TestInsertNewAnswerSubmission();
            TestGetProblemByID();
            TestGetAnswerSubmissionsByID();

            //getting the problem used in this test
            Problem problem = DBConnector.GetProblemByID(3);
            int ogSize = DBConnector.GetAnswerSubmissionsByID(user.Id).Count;
            //set reset user
            StandardUser resestUser = DBConnector.GetStandardUserByUsername(user.Username);

            //this is disgusting, but if you just send in dateTime.Now, *technically* the date returned by the DB is earlier than the actual date time, since
            //the dateTime we have here would have milliseconds as well....took me some time to figure out why it wasnt passing
            DateTime dateTime = DateTime.Now;
            DateTime actualDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            AnswerSubmission insertSubmission = new AnswerSubmission("This should be deleted", actualDateTime, user.Id, true, problem.ProblemNumber);

            //insert new answer
            DBConnector.InsertNewAnswerSubmission(insertSubmission, resestUser.PointsTotal, problem.PointsValue);

            //make sure everything was updated properly
            Assert.True(DBConnector.GetStandardUserByUsername(user.Username).PointsTotal == resestUser.PointsTotal+1);
            Problem updatedProblem = DBConnector.GetProblemByID(3);
            Assert.True(problem.Attempts + 1 == updatedProblem.Attempts && problem.Completions + 1 == updatedProblem.Completions);

            //make sure everything was reset properly
            DBConnector.ResetProblemSubmissions(DBConnector.GetStandardUserByUsername(resestUser.Username), problem.ProblemNumber);
            Assert.True(DBConnector.GetStandardUserByUsername(resestUser.Username).IsEqual(resestUser));
            Assert.True(DBConnector.GetProblemByID(updatedProblem.ProblemNumber).IsEqual(problem));
            Assert.True(DBConnector.GetAnswerSubmissionsByID(resestUser.Id).Count == ogSize);
        }

        //create
        [Test]
        public void TestCreateUserSubmissionTable()
        {
            //test query dependencies
            TestCheckIfUserSubmissionTableExists();
            
            //assure table does not exist already
            Assert.False(DBConnector.CheckIfUserSubmissionTableExists(0));

            //create table
            DBConnector.CreateUserSubmissionTable(0);
            Assert.True(DBConnector.CheckIfUserSubmissionTableExists(0));
            
            //delete table
            DBConnector.DropUserSubmissionTable(0);
            Assert.False(DBConnector.CheckIfUserSubmissionTableExists(0));
        }

    }
}
