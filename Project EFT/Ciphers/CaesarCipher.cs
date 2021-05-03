using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    /// <summary>Implements the necessary components, as outlined in <see cref="Cipher"/>, to perform Caesar Cipher encryption and decryption.
    /// Including, but not limited to, Caesar-specific form option generation, validation, and encryption and decryption execution.</summary>
    public class CaesarCipher : Cipher
    {
        /// <summary>See <see cref="Cipher.Name"/>.</summary>
        public override string Name { get { return "Caesar Cipher"; } }

        /// <summary>The encryption form index that includes the <see cref="NumberFieldOption"/> whose value is the shift amount when decrypting or encrypting.</summary>
        public const int ShiftIndex = 2;

        /// <summary>The <see cref="Option{T}.Value"/> of the decryption form's <see cref="RadioOptionsSet"/> when the user wishes to decrypt with a known shift amount.</summary>
        private const int KnownShiftChoice = 0;

        /// <summary>The index within the decryption form <see cref="RadioOptionsSet"/> of the <see cref="NumberFieldOption"/> whose <see cref="Option{T}.Value"/> is the decryption shift amount.</summary>
        private const int KnownShiftIndex = 0;

        /// <summary>The decryption form index for the <see cref="RadioOptionsSet"/> that contains the decryption method <see cref="Option"/>s.</summary>
        public const int DecryptionMethodIndex = 2;

        /// <summary>Creates an instance that returns the given number of solutions when decrypting.</summary>
        /// <param name="numSols">The number of solutions to return when decrypting.</param>
        public CaesarCipher(int numSols) : base(numSols){}

        /// <summary>Creates an instance that returns one solution when decrypting.</summary>
        public CaesarCipher() : base() { }

        /// <summary>
        /// Adds the Caesar-specific encryption and decryption <see cref="Option"/>s to the encryption and decryption options lists:<br/>
        /// Encryption: A <see cref="NumberFieldOption"/> whose <see cref="Option{T}.Value"/> is the encryption shift amount.<br/>
        /// Decryption: A <see cref="RadioOptionsSet"/> which contains a <see cref="NumberFieldOption"/> whose <see cref="Option{T}.Value"/> is the decryption shift amount, 
        /// and a <see cref="SimpleRadioStringOption"/> indicating the user does not know the shift amount.</summary>
        public override void GenerateForms() 
        {
            EncryptionFormOptions.Add(new NumberFieldOption("encryptionShiftAmount", "Enter shift amount", 3));
            DecryptionFormOptions.Add(new RadioOptionsSet("Decryption-Selection", null, 0, new List<Option>() {
                new NumberFieldOption("decryptionShiftAmount", "Enter shift amount", 3),
                new SimpleRadioStringOption("Unknown shift amount")
            }));
        }

        /// <summary>Performs Caesar cipher encryption and stores the result in the decryption form input <see cref="TextAreaOption"/>.</summary>
        protected override void Encrypt()
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

        /// <summary>Performs Caesar cipher decryption and stores the result in the encryption form input <see cref="TextAreaOption"/> - currently only works for a known shift amount.</summary>
        protected override void Decrypt()
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

        /// <summary>Performs validation of necessary encryption form data, including:<br/>
        /// - Input text and alphabet size validation.<br/>
        /// If an error is present, the appropriate error message is stored in the appropriate form option.</summary>
        /// <returns>True if Caesar encryption data is valid and encryption can occur, false otherwise.</returns>
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
        /// <returns>True if Caesar decryption data is valid and decryption can occur, false otherwise.</returns>
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
