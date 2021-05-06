using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    /// <summary>Implements the necessary components, as outlined in <see cref="Cipher"/>, to perform Pig Latin 'encryption' and 'decryption'.
    /// Including, but not limited to, Pig Latin-specific form option generation, validation, and encryption and decryption execution.</summary>
    public class PigLatin : Cipher
    {
        /// <summary>See <see cref="Cipher.Name"/>.</summary>
        public override string Name { get { return "Pig Latin"; } }

        /// <summary>Creates an instance that returns the given number of solutions when decrypting.</summary>
        /// <param name="numSols">The number of solutions to return when decrypting.</param>
        public PigLatin(int numSols) : base(numSols){}

        /// <summary>Creates an instance that returns one solution when decrypting.</summary>
        public PigLatin() : base() { }

        /// <summary>
        /// Adds the Pig Latin-specific encryption and decryption <see cref="Option"/>s to the encryption and decryption options lists.</summary>
        public override void GenerateForms(){ }

        /// <summary>Performs Pig Latin cipher encryption and stores the result in the decryption form input <see cref="TextAreaOption"/>.</summary>
        protected override void Encrypt()
        {
            string ciphertext = "";
            string plaintext = (string)EncryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();

            int wordStart = 0;
            int wordEnd = 0;
            string word;
            for (int i = 0; i < plaintext.Length; i++)
            {
                if (alphabet.IndexOf(plaintext[i]) == -1)
                {
                    wordEnd = i;
                    if (wordEnd > wordStart) 
                    {
                        word = plaintext.Substring(wordStart, wordEnd - wordStart).Trim();
                        word = word.Substring(1) + word[0] + "ay";
                        if (!(word == "" || word == null)) ciphertext += (word + " ");
                        wordStart = wordEnd + 1;
                    }
                }
            }
            if (wordStart < plaintext.Length) 
            {
                word = plaintext.Substring(wordStart, plaintext.Length - wordStart).Trim();
                if(word.Length>1)
                    word = word.Substring(1) + word[0] + "ay";
                ciphertext += word;
            }
            DecryptionFormOptions[InputIndex].SetValue(ciphertext);
        }

        /// <summary>Performs Pig Latin cipher decryption and stores the result in the encryption form input <see cref="TextAreaOption"/> - currently only works for a known shift amount.</summary>
        protected override void Decrypt()
        {
            
            string[] plaintexts = new string[NumSolutionsToReturn];
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();

            int wordStart = 0;
            int wordEnd = 0;
            string word;
            for (int i = 0; i < ciphertext.Length; i++)
            {
                if (alphabet.IndexOf(ciphertext[i]) == -1)
                {
                    wordEnd = i;
                    if (wordEnd > wordStart)
                    {
                        word = ciphertext.Substring(wordStart, wordEnd - wordStart).Trim();
                        if (word.Length > 1) word = word[word.Length - 3] + word.Substring(0, word.Length - 3);
                        if (!(word == "" || word == null)) plaintexts[0] += (word + " ");
                        wordStart = wordEnd + 1;
                    }
                }
            }
            if (wordStart < ciphertext.Length)
            {
                word = ciphertext.Substring(wordStart, ciphertext.Length - wordStart).Trim();
                if (word.Length > 1) word = word[word.Length - 3] + word.Substring(0, word.Length - 3);
                if (!(word == "" || word == null)) plaintexts[0] += (word + " ");
            }
            EncryptionFormOptions[InputIndex].SetValue(plaintexts[0]);
        }

        /// <summary>Performs validation of necessary encryption form data, including:<br/>
        /// - Input text and alphabet size validation.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if Pig Latin encryption data is valid and encryption can occur, false otherwise.</returns>
        protected override bool EncryptionFormDataIsValid()
        {
            bool error = false;
            if (!IsValidTextInput((string)EncryptionFormOptions[InputIndex].GetValue()))
            {
                EncryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                error = true;
            }
            if (!IsValidAlphabet((string)EncryptionFormOptions[AlphabetIndex].GetValue()))
            {
                EncryptionFormOptions[AlphabetIndex].ErrorMessage = InvalidAlphabetMessage;
                error = true;
            }
            return !error;
        }

        /// <summary>Performs validation of necessary decryption form data, including:<br/>
        /// - Input text and alphabet size validation.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if Pig Latin decryption data is valid and decryption can occur, false otherwise.</returns>
        protected override bool DecryptionFormDataIsValid()
        {
            bool error = false;
            if (!IsValidTextInput((string)DecryptionFormOptions[InputIndex].GetValue()))
            {
                EncryptionFormOptions[InputIndex].ErrorMessage = InvalidInputMessage;
                error = true;
            }
            if (!IsValidAlphabet((string)DecryptionFormOptions[AlphabetIndex].GetValue()))
            {
                EncryptionFormOptions[AlphabetIndex].ErrorMessage = InvalidAlphabetMessage;
                error = true;
            }
            return !error;
        }
    }
}
