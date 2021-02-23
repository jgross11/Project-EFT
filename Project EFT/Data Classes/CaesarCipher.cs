using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_EFT.Data_Classes
{
    // data class representing a Caesar Cipher
    public class CaesarCipher : Cipher
    {
        public int decryptionShiftAmount { get; set; }
        public int encryptionShiftAmount { get; set; }

        // null constructor for convenience / piecewise field setting
        public CaesarCipher() 
        { 
            
        }

        public CaesarCipher(string alphabet, int dec, int enc, int numSols) 
        {
            this.alphabet = alphabet;
            decryptionShiftAmount = dec;
            encryptionShiftAmount = enc;
            numSolutionsToReturn = numSols;
        }

        public override string Encrypt(string plaintext)
        {
            string ciphertext = "";
            foreach (char c in plaintext) 
            {
                // ignore characters not in alphabet (spaces, periods, etc.)
                int index = alphabet.IndexOf(c);
                if (index == -1)
                {
                    ciphertext += c;
                }
                // otherwise, do shift
                else
                {
                    // TODO this doesn't work for negative shift values
                    ciphertext += alphabet[(index + encryptionShiftAmount) % alphabet.Length];
                }
            }
            return ciphertext;
        }

        public override string[] Decrypt(string ciphertext)
        {
            string[] plaintexts = new string[numSolutionsToReturn];
            foreach (char c in ciphertext) 
            {
                int index = alphabet.IndexOf(c);
                if (index == -1) 
                {
                    plaintexts[0] += c;
                }
                else 
                {
                    plaintexts[0] += alphabet[(index - decryptionShiftAmount) % alphabet.Length];
                }
            }
            return plaintexts;
        }
    }
}
