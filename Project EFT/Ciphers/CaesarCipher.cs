using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    // data class representing a Caesar Cipher
    public class CaesarCipher : Cipher
    {
        public override string Name { get { return "Caesar Cipher"; } }
        private const int ShiftIndex = 2;

        public CaesarCipher(int numSols) : base(numSols)
        {
            
        }

        public CaesarCipher() : base() { }

        public override void GenerateForms() 
        {
            EncryptionFormOptions.Add(new NumberFieldOption("encryptionShiftAmount", "Enter shift amount", 3));
            DecryptionFormOptions.Add(new NumberFieldOption("decryptionShiftAmount", "Enter shift amount", 3));
        }

        public override string Encrypt()
        {
            string ciphertext = "";
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            int encryptionShiftAmount = (int)EncryptionFormOptions[ShiftIndex].GetValue();

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
                    int newPosition = (((index + encryptionShiftAmount) % alphabet.Length) + alphabet.Length) % alphabet.Length;
                    ciphertext += alphabet[newPosition];
                }
            }
            // TODO store in appropriate option
            DecryptionFormOptions[InputIndex].SetValue(ciphertext);
            return ciphertext;
        }

        public override string[] Decrypt()
        {
            string[] plaintexts = new string[NumSolutionsToReturn];
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();
            int decryptionShiftAmount = (int)DecryptionFormOptions[ShiftIndex].GetValue();
            foreach (char c in ciphertext) 
            {
                int index = alphabet.IndexOf(c);
                if (index == -1) 
                {
                    plaintexts[0] += c;
                }
                else 
                {
                    int newPosition = (((index - decryptionShiftAmount) % alphabet.Length) + alphabet.Length) % alphabet.Length;
                    plaintexts[0] += alphabet[newPosition];
                }
            }
            // TODO store in appropriate option
            EncryptionFormOptions[InputIndex].SetValue(plaintexts[0]);
            return plaintexts;
        }
    }
}
