using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    /// <summary>Implements the necessary components, as outlined in <see cref="Cipher"/>, to perform Vigenere encryption and decryption.
    /// Including, but not limited to, Vigenere-specific form option generation, validation, and encryption and decryption execution.</summary>
    public class Vigenere : Cipher
    {
        /// <summary>See <see cref="Cipher.Name"/>.</summary>
        public override string Name { get { return "Vigenère Cipher"; } }

        /// <summary>The encryption form index that includes the <see cref="TextBoxOption"/> whose value is the key to use when encrypting.</summary>
        public const int KeyIndex = 2;

        /// <summary>The decryption form index for the <see cref="RadioOptionsSet"/> that contains the decryption method <see cref="Option"/>s.</summary>
        public const int DecryptionMethodIndex = 2;

        /// <summary>The <see cref="Option{T}.Value"/> of the decryption form's <see cref="RadioOptionsSet"/> when the user wishes to decrypt with a fully known key.</summary>
        public const int FullKeyDecryptionChoice = 0;

        /// <summary>The <see cref="Option{T}.Value"/> of the decryption form's <see cref="RadioOptionsSet"/> when the user wishes to decrypt with a partially known key.</summary>
        public const int PartialKeyDecryptionChoice = 1;

        /// <summary>The <see cref="Option{T}.Value"/> of the decryption form's <see cref="RadioOptionsSet"/> when the user wishes to decrypt with an unknown key.</summary>
        public const int UnknownKeyDecryptionChoice = 2;

        /// <summary>Creates an instance that returns the given number of solutions when decrypting.</summary>
        /// <param name="numSols">The number of solutions to return when decrypting.</param>
        public Vigenere(int numSols) : base(numSols)
        {

        }

        /// <summary>Creates an instance that returns one solution when decrypting.</summary>
        public Vigenere() : base() { }

        /// <summary>
        /// Adds the Vigenere-specific encryption and decryption <see cref="Option"/>s to the encryption and decryption options list:<br/>
        /// Encryption: A <see cref="TextBoxOption"/> whose <see cref="Option{T}.Value"/> is the encryption key.<br/>
        /// Decryption: A <see cref="RadioOptionsSet"/> which contains a <see cref="TextBoxOption"/> whose <see cref="Option{T}.Value"/> is a fully known key,
        /// a <see cref="TextBoxOption"/> whose <see cref="Option{T}.Value"/> is a partially known key,
        /// and a <see cref="SimpleRadioStringOption"/> indicating the user does not know anything about the key.</summary>
        public override void GenerateForms()
        {
            EncryptionFormOptions.Add(new TextBoxOption("substitutionKey", "Enter key", ""));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-selection", null, 0, new List<Option> {
                new TextBoxOption("substitutionKey", "Enter key", ""),
                new TextBoxOption("partialKey", "Enter partially known key", ""),
                new SimpleRadioStringOption("Unknown key")
            }));
        }

        /// <summary>Performs Vigenere cipher encryption and stores the result in the decryption form input <see cref="TextAreaOption"/>.</summary>
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

        /// <summary>Performs Vigenere cipher decryption and stores the result in the encryption form input <see cref="TextAreaOption"/> - currently only works for a fully known key.</summary>
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

        /// <summary>Performs validation of necessary encryption form data, including:<br/>
        /// - Input text, alphabet, and key size validation.<br/>
        /// - Ensuring the alphabet contains every character in the key.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if Vigenere encryption data is valid and encryption can occur, false otherwise.</returns>
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

        /// <summary>Performs validation of necessary decryption form data, including:<br/>
        /// - Input text and base alphabet size validation.<br/>
        /// - If decrypting with a partially or fully known key, key size validation and ensuring the alphabet contains every key character. <br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if Vigenere decryption data is valid and decryption can occur, false otherwise.</returns>
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
