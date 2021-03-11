using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    public abstract class Cipher
    {
        public int numSolutionsToReturn {get; set;}
        public string name { get; set; }
        public string alphabet;
        public abstract string Encrypt(string plaintext);
        public abstract string[] Decrypt(string ciphertext);
    }
}
