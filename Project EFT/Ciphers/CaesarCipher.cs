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
        private const int KnownShiftChoice = 0;
        private const int KnownShiftIndex = 0;
        private const int DecryptionMethodIndex = 2;

        public CaesarCipher(int numSols) : base(numSols)
        {
            
        }

        public CaesarCipher() : base() { }

        public override void GenerateForms() 
        {
            EncryptionFormOptions.Add(new NumberFieldOption("encryptionShiftAmount", "Enter shift amount", 3));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-Selection", null, 0, new List<Option>() {
                new NumberFieldOption("decryptionShiftAmount", "Enter shift amount", 3),
                new SimpleRadioStringOption("Unknown shift amount")
            }));
        }

        public override void Encrypt()
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
                    int newPosition = Mod(index + encryptionShiftAmount, alphabet.Length);
                    ciphertext += alphabet[newPosition];
                }
            }
            // TODO store in appropriate option
            DecryptionFormOptions[InputIndex].SetValue(ciphertext);
        }

        public override void Decrypt()
        {
            string[] plaintexts = new string[NumSolutionsToReturn];
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();
            int decryptionMethod = (int)DecryptionFormOptions[DecryptionMethodIndex].GetValue();
            if (decryptionMethod == KnownShiftChoice)
            {
                int decryptionShiftAmount = (int)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[KnownShiftIndex].GetValue();
                foreach (char c in ciphertext)
                {
                    int index = alphabet.IndexOf(c);
                    if (index == -1)
                    {
                        plaintexts[0] += c;
                    }
                    else
                    {
                        int newPosition = Mod(index - decryptionShiftAmount, alphabet.Length);
                        plaintexts[0] += alphabet[newPosition];
                    }
                }
                // TODO store in appropriate option
                EncryptionFormOptions[InputIndex].SetValue(plaintexts[0]);
            }
            else 
            {
                EncryptionFormOptions[InputIndex].SetValue("TODO decrypt without knowing shift amount!!!");
            }
        }
    }
}
