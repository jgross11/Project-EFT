using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    /// <summary>Contains helper methods and constants for information validation purposes, including email, username, password validation, and error messages pertaining to validation. <br/>
    /// Also contains methods to generate and hash temporary passwords to be used when recovering account information.
    /// </summary>
    public static class InformationValidator
    {
        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating an email.</summary>
        public const int EmailType = 1;

        /// <summary>Maximum length allowed for an email address.</summary>
        private const int MaxEmailLength = 45;

        /// <summary>Error message to display when email validation fails.</summary>
        public const string InvalidEmailString = "Please enter a properly formatted email between 5 and 45 characters.";

        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating a password.</summary>
        public const int PasswordType = 2;

        /// <summary>Length of the hashed output of <see cref="MD5Hash(string)"/>. </summary>
        private const int HashPasswordLength = 32;

        /// <summary>Error message to display when password validation fails.</summary>
        public const string InvalidPasswordString = "Please enter a valid password between 8 and 40 characters, consisting of at least one uppercase letter, one number, and one special character.";

        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating a username.</summary>
        public const int UsernameType = 3;

        /// <summary>Minimum length allowed for a username.</summary>
        private const int MinimumUsernameLength = 1;

        /// <summary>Maximum length allowed for a username.</summary>
        private const int MaximumUsernameLength = 45;

        /// <summary>Error message to display when username validation fails.</summary>
        public const string InvalidUsernameString = "Please enter a valid username between 1 and 45 characters.";

        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating a problem submission.</summary>
        public const int ProblemSubmissionType = 4;

        /// <summary>Maximum length allowed for a problem submission.</summary>
        private const int MaximumProblemSubmissionLength = 255;

        /// <summary>Error message to display when problem submission validation fails.</summary>
        public const string InvalidProblemSubmissionString = "Please enter a valid submission between 1 and 255 characters.";

        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating a problem title.</summary>
        public const int ProblemTitleType = 5;

        /// <summary>Maximum length allowed for a problem title.</summary>
        private const int MaximumProblemTitleLength = 40;

        /// <summary>Error message to display when problem title validation fails.</summary>
        public const string InvalidProblemTitleString = "Please enter a valid problem title between 1 and 40 characters.";

        /// <summary>Parameter value to be passed in <see cref="VerifyInformation(string, int)"/> when validating a problem points value.</summary>
        public const int ProblemValueType = 6;

        /// <summary>Minimum problem worth value.</summary>
        private const int MinProblemValue = 1;

        /// <summary>Maximum problem worth value.</summary>
        private const int MaxProblemValue = 5;

        /// <summary>Error message to display when problem value validation fails.</summary>
        public const string InvalidProblemValueString = "Please enter a valid point value between 1 and 5.";

        /// <summary>All English letters in lowercase, to be used in <see cref="GenerateTemporaryPassword"/>.</summary>
        private const string Lowers = "qwertyuiopasdfghjklzxcvbnm";

        /// <summary>All English letters in uppercase, to be used in <see cref="GenerateTemporaryPassword"/>.</summary>
        private const string Uppers = "MNBVCXZQWERTYUIOPHJKLFDSAG";

        /// <summary>All digits, to be used in <see cref="GenerateTemporaryPassword"/>.</summary>
        private const string Numbers = "0512864973";

        /// <summary>An assortment of symbols, to be used in <see cref="GenerateTemporaryPassword"/>.</summary>
        private const string Symbols = ">/?=*@[}^&+.$\\|]~-()`#!_%";

        /// <summary>Verifies the given information according to its information type.</summary>
        /// <param name="info">The information to verify.</param>
        /// <param name="type">The type of information the given information is. Common values include <see cref="EmailType"/>, <see cref="UsernameType"/>.</param>
        /// <returns>True if the information is valid according to its specifications, false otherwise.</returns>
        public static bool VerifyInformation(string info, int type) 
        {
            if (info == null || info.Length == 0) return false;
            string testInfo = null;
            switch (type) 
            {
                case EmailType:
                    testInfo = info.Trim();
                    if (testInfo.Length > MaxEmailLength || testInfo.Length < 5) return false;
                    bool foundAt = false;
                    for (int i = 1; i < testInfo.Length; i++) 
                    {
                        char c = testInfo[i];
                        if (c == '@' && !foundAt) foundAt = true;
                        else if (foundAt && c == '.' && testInfo[i - 1] != '@' && i != testInfo.Length - 1) return true;
                        else if (c == '@' && foundAt)
                        {
                            return false;
                        }
                    }
                    return false;
                case PasswordType:
                    if (info.Length != HashPasswordLength) return false;
                    foreach (char c in info) 
                    {
                        if (!"0123456789".Contains(c) && !"abcdef".Contains(c) && !"ABCDEF".Contains(c)) 
                        {
                            return false;
                        }
                    }
                    return true;
                case UsernameType:
                    testInfo = info.Trim();
                    return testInfo.Length >= MinimumUsernameLength && testInfo.Length <= MaximumUsernameLength;
                case ProblemSubmissionType:
                    testInfo = info.Trim();
                    return testInfo.Length > 0 && testInfo.Length <= MaximumProblemSubmissionLength;
                case ProblemTitleType:
                    testInfo = info.Trim();
                    return testInfo.Length > 0 && testInfo.Length <= MaximumProblemTitleLength;
                case ProblemValueType:
                    try
                    {
                        int actualValue = int.Parse(info);
                        return actualValue >= MinProblemValue && actualValue <= MaxProblemValue;
                    }
                    catch { return false; }
            }

            return false;
        }

        /// <summary>Randomly generates a password meeting the criteria that a password for an account must satisfy.</summary>
        /// <returns>A 16 character long string containing 4 lowercase letters, 4 uppercase letters, 4 numbers, and 4 symbols, randomly selected in a random ordering.</returns>
        public static string GenerateTemporaryPassword()
        {

            // number of each type of character
            int numEach = 4;

            // number of categories of characters
            int numCategoriesRemaining = 4;

            // number of characters remaining for each category
            int[] numRemaining = { numEach, numEach, numEach, numEach };

            string result = "";

            // array containing arrays of character types
            string[] chars = { Lowers, Uppers, Numbers, Symbols };
            Random rand = new Random();
            for (int i = 0; i < numEach * 4; i++)
            {
                int categoryIndex = (int) Math.Round(rand.NextDouble() * (numCategoriesRemaining - 1));
                result += chars[categoryIndex][(int)Math.Round(rand.NextDouble() * (chars[categoryIndex].Length - 1))];
                numRemaining[categoryIndex]--;
                if (numRemaining[categoryIndex] == 0)
                {
                    int temp = numRemaining[numCategoriesRemaining - 1];
                    numRemaining[numCategoriesRemaining - 1] = numRemaining[categoryIndex];
                    numRemaining[categoryIndex] = temp;

                    string tempString = chars[numCategoriesRemaining - 1];
                    chars[numCategoriesRemaining - 1] = chars[categoryIndex];
                    chars[categoryIndex] = tempString;
                    numCategoriesRemaining--;
                }
            }
            return result;
        }

        /// <summary>Performs MD5 hashing on the given input.</summary>
        /// <param name="plain">The plaintext string to hash.</param>
        /// <returns>A 32 character long string representing the MD5 output in hex form.</returns>
        public static string MD5Hash(string plain) 
        {
            string result = "";
            MD5 md5 = MD5.Create();
            foreach (byte b in md5.ComputeHash(Encoding.ASCII.GetBytes(plain)))
            {
                result += b.ToString("x2");
            }
            md5.Dispose();
            return result;
        }
    }
}
