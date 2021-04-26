using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public class Vigenere : Cipher
    {
        public override string Name { get { return "Vigenère Cipher"; } }
        public const int KeyIndex = 2;
        public const int DecryptionMethodIndex = 2;
        public const int FullKeyDecryptionChoice = 0;
        public const int PartialKeyDecryptionChoice = 1;
        public const int UnknownKeyDecryptionChoice = 2;

        public Vigenere(int numSols) : base(numSols)
        {

        }

        public Vigenere() : base() { }

        public override void GenerateForms()
        {
            EncryptionFormOptions.Add(new TextBoxOption("substitutionKey", "Enter key", ""));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-selection", null, 0, new List<Option> {
                new TextBoxOption("substitutionKey", "Enter key", ""),
                new TextBoxOption("partialKey", "Enter partially known key", ""),
                new SimpleRadioStringOption("Unknown key")
            }));
        }

        protected override void Encrypt()
        {
            string ciphertext = "";
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            string key = (string)EncryptionFormOptions[KeyIndex].GetValue();
            int keyPosition = 0;
            for (int i = 0; i < plaintext.Length; i++) 
            {
                int keyIndex = alphabet.IndexOf(key[keyPosition]);
                if (keyIndex == -1) ciphertext += plaintext[i];
                else 
                {
                    int characterIndex = alphabet.IndexOf(plaintext[i]);
                    if (characterIndex == -1) ciphertext += plaintext[i];
                    else
                    {
                        ciphertext += alphabet[Mod(characterIndex + keyIndex, alphabet.Length)];
                        keyPosition = (keyPosition + 1) % key.Length;
                    }
                }
            }

            // TODO store in appropriate option
            DecryptionFormOptions[InputIndex].SetValue(ciphertext);
        }

        protected override void Decrypt()
        {
            
            string[] plaintexts = new string[NumSolutionsToReturn];
            int decryptionMethod = (int) DecryptionFormOptions[DecryptionMethodIndex].GetValue();
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();

            if (decryptionMethod == FullKeyDecryptionChoice)
            {
                string key = (string)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[FullKeyDecryptionChoice].GetValue();
                int keyPosition = 0;
                for (int i = 0; i < ciphertext.Length; i++)
                {
                    int keyIndex = alphabet.IndexOf(key[keyPosition]);
                    if (keyIndex == -1) plaintexts[0] += ciphertext[i];
                    else
                    {
                        int characterIndex = alphabet.IndexOf(ciphertext[i]);
                        if (characterIndex == -1) plaintexts[0] += ciphertext[i];
                        else
                        {
                            plaintexts[0] += alphabet[Mod(characterIndex - keyIndex, alphabet.Length)];
                            keyPosition = (keyPosition + 1) % key.Length;
                        }
                    }
                }
                // TODO store in appropriate option
                EncryptionFormOptions[InputIndex].SetValue(plaintexts[0]);
            }
            else if (decryptionMethod == PartialKeyDecryptionChoice)
            {
                EncryptionFormOptions[InputIndex].SetValue("TODO decrypt with partial key!!!");
            }
            else 
            {
                EncryptionFormOptions[InputIndex].SetValue("TODO decrypt with no knowledge of key!!!");
            }
            
        }

        protected override bool EncryptionFormDataIsValid()
        {
            bool error = false;
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            string key = (string)EncryptionFormOptions[KeyIndex].GetValue();

            if (!IsValidTextInput((string)EncryptionFormOptions[InputIndex].GetValue()))
            {
                EncryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                error = true;
            }
            if (!IsValidAlphabet(alphabet))
            {
                EncryptionFormOptions[AlphabetIndex].ErrorMessage = InvalidAlphabetMessage;
                error = true;
            }
            if (!IsValidTextInput(key))
            {
                EncryptionFormOptions[KeyIndex].ErrorMessage = InvalidKeyMessage;
                error = true;
            }

            if (!AlphabetContainsKey(alphabet, key))
            {
                EncryptionFormOptions[KeyIndex].ErrorMessage = MismatchedKeyAndAlphabetMessage;
                error = true;
            }
            return !error;
        }

        protected override bool DecryptionFormDataIsValid()
        {
            bool error = false;
            
            int decryptionMethod = (int)DecryptionFormOptions[DecryptionMethodIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();

            if (!IsValidTextInput((string)DecryptionFormOptions[InputIndex].GetValue()))
            {
                DecryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                error = true;
            }
            if (!IsValidAlphabet(alphabet))
            {
                DecryptionFormOptions[AlphabetIndex].ErrorMessage = InvalidAlphabetMessage;
                error = true;
            }
            if (decryptionMethod == FullKeyDecryptionChoice || decryptionMethod == PartialKeyDecryptionChoice)
            {
                string key = (string)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[decryptionMethod].GetValue();
                if (!IsValidTextInput(key))
                {
                    DecryptionFormOptions[DecryptionMethodIndex].ErrorMessage = InvalidKeyMessage;
                    error = true;
                }
                if (!AlphabetContainsKey(alphabet, key))
                {
                    DecryptionFormOptions[DecryptionMethodIndex].ErrorMessage = MismatchedKeyAndAlphabetMessage;
                    error = true;
                }
            }
            
            return !error;
        }
    }
}
