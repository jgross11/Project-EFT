using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public class MASC : Cipher
    {
        public override string Name { get { return "Monoalphabetic Substitution Cipher (MASC)"; } }
        private const int SubstitutionAlphabetIndex = 2;

        public MASC(int numSols) : base(numSols)
        {

        }

        public MASC() : base() { }

        public override void GenerateForms()
        {
            EncryptionFormOptions.Add(new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"));
            DecryptionFormOptions.Add(new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"));
        }

        public override string Encrypt()
        {
            string ciphertext = "";
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            string substitutionAlphabet = (string)EncryptionFormOptions[SubstitutionAlphabetIndex].GetValue();

            foreach (char c in plaintext)
            {
                // ignore characters not in alphabet (spaces, periods, etc.)
                int index = alphabet.IndexOf(c);

                // TODO uneven alphabet sizes test
                if (index == -1)
                {
                    // TODO error / messages
                    ciphertext += c;
                }
                // otherwise, do shift
                else
                {
                    ciphertext += substitutionAlphabet[index];
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
            string substitutionAlphabet = (string)DecryptionFormOptions[SubstitutionAlphabetIndex].GetValue();
            foreach (char c in ciphertext)
            {
                int index = substitutionAlphabet.IndexOf(c);
                if (index == -1)
                {
                    plaintexts[0] += c;
                }
                else
                {
                    plaintexts[0] += alphabet[index];
                }
            }
            // TODO store in appropriate option
            EncryptionFormOptions[InputIndex].SetValue(plaintexts[0]);
            return plaintexts;
        }
    }
}
