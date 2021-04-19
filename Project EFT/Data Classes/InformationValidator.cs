using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public static class InformationValidator
    {
        public const int EmailType = 1;
        private const int MaxEmailLength = 45;
        public const string InvalidEmailString = "Please enter a properly formatted email between 5 and 45 characters.";
        public const int PasswordType = 2;
        private const int MinimumPasswordLength = 8;
        private const int MaximumPasswordLength = 40;
        public const string InvalidPasswordString = "Please enter a valid password between 8 and 40 characters, consisting of at least one uppercase letter, one number, and one special character.";
        public const int UsernameType = 3;
        private const int MinimumUsernameLength = 1;
        private const int MaximumUsernameLength = 45;
        public const string InvalidUsernameString = "Please enter a valid username between 1 and 45 characters.";
        public const int ProblemSubmissionType = 4;
        private const int MaximumProblemSubmissionLength = 255;
        public const string InvalidProblemSubmissionString = "Please enter a valid submission between 1 and 255 characters.";
        public const int ProblemTitleType = 5;
        private const int MaximumProblemTitleLength = 40;
        public const string InvalidProblemTitleString = "Please enter a valid problem title between 1 and 40 characters.";
        public const int ProblemValueType = 6;
        private const int MinProblemValue = 1;
        private const int MaxProblemValue = 5;
        public const string InvalidProblemValueString = "Please enter a valid point value between 1 and 5.";

        private const string Lowers = "qwertyuiopasdfghjklzxcvbnm";
        private const string Uppers = "MNBVCXZQWERTYUIOPHJKLFDSAG";
        private const string Numbers = "0512864973";
        private const string Symbols = ">/?=*@[}^&+.$\\|]~-()`#!_%";

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
                    if (info.Length > MaximumPasswordLength) return false;
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
