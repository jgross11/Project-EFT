using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public class MASC : Cipher
    {
        public override string Name { get { return "Monoalphabetic Substitution Cipher (MASC)"; } }
        private const int SubstitutionAlphabetIndex = 2;
        private const int DecryptionMethodIndex = 2;
        private const int KnownSubstitutionAlphabetChoice = 0;
        private const int DecryptionSubstitutionAlphabetIndex = 0;

        public MASC(int numSols) : base(numSols)
        {

        }

        public MASC() : base() { }

        public override void GenerateForms()
        {
            EncryptionFormOptions.Add(new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-selection", null, 0, new List<Option> {
                new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"),
                new SimpleRadioStringOption("Unknown substitution alphabet")
            }));
        }

        public override void Encrypt()
        {
            string ciphertext = "";
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            string substitutionAlphabet = (string)EncryptionFormOptions[SubstitutionAlphabetIndex].GetValue();

            if (alphabet.Length != substitutionAlphabet.Length) 
            {
                EncryptionFormOptions[SubstitutionAlphabetIndex].ErrorMessage = 
                    String.Format("Please ensure alphabets contain the same number of characters (Base: {0} characters, Substitution: {1} characters)", alphabet.Length, substitutionAlphabet.Length);
                return;
            }

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
        }

        public override void Decrypt()
        {
            string[] plaintexts = new string[NumSolutionsToReturn];
            int decryptionMethod = (int) DecryptionFormOptions[DecryptionMethodIndex].GetValue();
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();

            if (decryptionMethod == KnownSubstitutionAlphabetChoice)
            {
                string substitutionAlphabet = (string)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[DecryptionSubstitutionAlphabetIndex].GetValue();
                if (alphabet.Length != substitutionAlphabet.Length)
                {
                    DecryptionFormOptions[SubstitutionAlphabetIndex].ErrorMessage =
                        String.Format("Please ensure alphabets contain the same number of characters (Base: {0} characters, Substitution: {1} characters)", alphabet.Length, substitutionAlphabet.Length);
                    return;
                }

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
            }
            else 
            {
                EncryptionFormOptions[InputIndex].SetValue("TODO decrypt without knowing alphabet!!!");
            }
        }
    }
}
