using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    /// <summary>Implements the necessary components, as outlined in <see cref="Cipher"/>, to perform monoalphabetic substitution encryption and decryption.
    /// Including, but not limited to, MASC-specific form option generation, validation, and encryption and decryption execution.</summary>
    public class MASC : Cipher
    {
        /// <summary>See <see cref="Cipher.Name"/>.</summary>
        public override string Name { get { return "Monoalphabetic Substitution Cipher (MASC)"; } }

        /// <summary>The encryption form index of the <see cref="TextBoxOption"/> whose value is the substitution alphabet to use when encrypting.</summary>
        private const int SubstitutionAlphabetIndex = 2;

        /// <summary>The decryption form index of the <see cref="RadioOptionsSet"/> whose value is the selected decryption method.</summary>
        private const int DecryptionMethodIndex = 2;

        /// <summary>The selected value of the <see cref="RadioOptionsSet"/> in the decryption options list when decrypting with a known substitution alphabet.</summary>
        private const int KnownSubstitutionAlphabetChoice = 0;

        /// <summary>The index in the <see cref="RadioOptionsSet"/> in the decryption options list of the <see cref="TextBoxOption"/> whose value is the decryption substitution alphabet.</summary>
        private const int DecryptionSubstitutionAlphabetIndex = 0;

        /// <summary>Creates an instance that returns the given number of solutions when decrypting.</summary>
        /// <param name="numSols">The number of solutions to return when decrypting.</param>
        public MASC(int numSols) : base(numSols){}

        /// <summary>Creates an instance that returns one solution when decrypting.</summary>
        public MASC() : base() { }

        /// <summary>
        /// Adds the MASC-specific encryption and decryption <see cref="Option"/>s to the encryption and decryption options list:<br/>
        /// Encryption: A <see cref="TextBoxOption"/> whose <see cref="Option{T}.Value"/> is the encryption substitution alphabet.<br/>
        /// Decryption: A <see cref="RadioOptionsSet"/> which contains a <see cref="TextBoxOption"/> whose <see cref="Option{T}.Value"/> is the decryption substitution alphabet, 
        /// and a <see cref="SimpleRadioStringOption"/> indicating the user does not know the decryption substitution alphabet.</summary>
        public override void GenerateForms()
        {
            EncryptionFormOptions.Add(new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-selection", null, 0, new List<Option> {
                new TextBoxOption("substitutionAlphabet", "Enter substitution alphabet", "abcdefghijklmnopqrstuvwxyz"),
                new SimpleRadioStringOption("Unknown substitution alphabet")
            }));
        }

        /// <summary>Performs MASC encryption and stores the result in the decryption form input <see cref="TextAreaOption"/>.</summary>
        protected override void Encrypt()
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
        }

        /// <summary>Performs MASC encryption and stores the result in the encryption form input <see cref="TextAreaOption"/> - currently only works for a known substitution alphabet.</summary>
        protected override void Decrypt()
        {
            string[] plaintexts = new string[NumSolutionsToReturn];
            int decryptionMethod = (int) DecryptionFormOptions[DecryptionMethodIndex].GetValue();
            string ciphertext = (string)DecryptionFormOptions[InputIndex].GetValue();
            string alphabet = (string)DecryptionFormOptions[AlphabetIndex].GetValue();

            if (decryptionMethod == KnownSubstitutionAlphabetChoice)
            {
                string substitutionAlphabet = (string)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[DecryptionSubstitutionAlphabetIndex].GetValue();
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

        /// <summary>Performs validation of necessary encryption form data, including:<br/>
        /// - Input text, base alphabet, and substitution alphabet size validation.<br/>
        /// - Base alphabet and substitution alphabet length matching.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if MASC encryption data is valid and encryption can occur, false otherwise.</returns>
        protected override bool EncryptionFormDataIsValid()
        {
            bool error = false;
            string alphabet = (string)EncryptionFormOptions[AlphabetIndex].GetValue();
            string substitutionAlphabet = (string)EncryptionFormOptions[SubstitutionAlphabetIndex].GetValue();


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
            if (!IsValidAlphabet(substitutionAlphabet))
            {
                EncryptionFormOptions[SubstitutionAlphabetIndex].ErrorMessage = InvalidAlphabetMessage;
                error = true;
            }

            if (!AlphabetSizeMatch(alphabet, substitutionAlphabet))
            {
                EncryptionFormOptions[SubstitutionAlphabetIndex].ErrorMessage = FormatUnequalAlphabetMessage(alphabet, substitutionAlphabet);
                error = true;
            }
            return !error;
        }

        /// <summary>Performs validation of necessary decryption form data, including:<br/>
        /// - Input text and base alphabet size validation.<br/>
        /// - If decrypting with a known alphabet, substitution alphabet size validation and base alphabet and substitution alphabet length matching. <br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if MASC decryption data is valid and decryption can occur, false otherwise.</returns>
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
            if (decryptionMethod == KnownSubstitutionAlphabetChoice)
            {
                string substitutionAlphabet = (string)((RadioOptionsSet)DecryptionFormOptions[DecryptionMethodIndex]).Choices[DecryptionSubstitutionAlphabetIndex].GetValue();
                if (!IsValidAlphabet(substitutionAlphabet))
                {
                    DecryptionFormOptions[DecryptionMethodIndex].ErrorMessage = InvalidAlphabetMessage;
                    error = true;
                }
                if (!AlphabetSizeMatch(alphabet, substitutionAlphabet))
                {
                    DecryptionFormOptions[DecryptionMethodIndex].ErrorMessage = FormatUnequalAlphabetMessage(alphabet, substitutionAlphabet);
                    error = true;
                }
            }
            return !error;
        }
    }
}
