using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    /// <summary>Represents the commonalities that most cipher systems have, as well as the additional constraints imposed upon such systems by this architecture. 
    /// Responsible for creating, rendering, and interacting with a front end form containing everything necessary for a system to perform it's intended encryption and decryption.</summary>
    public abstract class Cipher
    {
        /// <summary>The number of solutions any decryption effort should return. Currently, all encryption efforts are assumed to return only one solution.</summary>
        public int NumSolutionsToReturn {get; set;}

        /// <summary>The name of the system, to be displayed on the Cipher and Ciphers List page. Must be overridden by every subclass.</summary>
        public virtual string Name { get; set; }

        /// <summary>Contains <see cref="NumSolutionsToReturn"/> decryption solutions when decrypting.</summary>
        public string[] outputs { get; set; }

        /// <summary>Contains every <see cref="Option"/> necessary for a cipher to perform encryption.</summary>
        public List<Option> EncryptionFormOptions;

        /// <summary>Contains every <see cref="Option"/> necessary for a cipher to perform decryption.</summary>
        public List<Option> DecryptionFormOptions;

        /// <summary>Implements the modification of <see cref="EncryptionFormOptions"/> and <see cref="DecryptionFormOptions"/> 
        /// for subclass specific encryption and decryption <see cref="Option"/>s necessary to perform subclass encryption and decryption.</summary>
        public abstract void GenerateForms();

        /// <summary>Implements the validation of data for the necessary <see cref="Option"/>s in <see cref="EncryptionFormOptions"/> 
        /// to determine if encryption is possible with the given data.</summary>
        /// <returns>True if all necessary encryption data is valid and encryption can occur, false otherwise.</returns>
        protected abstract bool EncryptionFormDataIsValid();

        /// <summary>Implements the validation of data for the necessary <see cref="Option"/>s in <see cref="DecryptionFormOptions"/> 
        /// to determine if decryption is possible with the given data.</summary>
        /// <returns>True if all necessary decryption data is valid and decryption can occur, false otherwise.</returns>
        protected abstract bool DecryptionFormDataIsValid();

        /// <summary>Only to be called after <see cref="EncryptionFormDataIsValid"/> has returned true. 
        /// Performs the subclass's encryption method, and stores the result in the appropriate <see cref="EncryptionFormOptions"/>
        /// option for displaying encryption result(s) on the front end.</summary>
        protected abstract void Encrypt();

        /// <summary>Only to be called after <see cref="DecryptionFormDataIsValid"/> has returned true. 
        /// Performs the subclass's decryption method, and stores the result in the appropriate <see cref="DecryptionFormOptions"/>
        /// option for displaying decryption result(s) on the front end.</summary>
        protected abstract void Decrypt();

        /// <summary>The index in <see cref="EncryptionFormOptions"/> and <see cref="DecryptionFormOptions"/> that stores the <see cref="Option"/>
        /// that contains the value of the user's plaintext when encrypting, or ciphertext when decrypting.</summary>
        public const int InputIndex = 0;

        /// <summary>The index in <see cref="EncryptionFormOptions"/> and <see cref="DecryptionFormOptions"/> that stores the <see cref="Option"/>
        /// that contains the value of the user's plaintext alphabet when encrypting, or ciphertext alphabet when decrypting.</summary>
        public const int AlphabetIndex = 1;

        /// <summary>The standard English alphabet in lowercase.</summary>
        public const string standardAlphabet = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>The error message to be placed in 
        /// (<see cref="EncryptionFormOptions"/> or <see cref="DecryptionFormOptions"/>)[<see cref="AlphabetIndex"/>] 
        /// when a submitted alphabet is empty.</summary>
        public const string InvalidAlphabetMessage = "Please ensure the alphabet contains at least one character.";

        /// <summary>The error message to be placed in 
        /// (<see cref="EncryptionFormOptions"/> or <see cref="DecryptionFormOptions"/>)[<see cref="AlphabetIndex"/>] 
        /// when a submitted alphabet does not contain every character that a submitted key does.</summary>
        public const string MismatchedKeyAndAlphabetMessage = "Please ensure the alphabet contains every character in the key.";

        /// <summary>The error message to be placed in 
        /// (<see cref="EncryptionFormOptions"/> or <see cref="DecryptionFormOptions"/>)[<see cref="InputIndex"/>] 
        /// when a submitted input is empty.</summary>
        public const string InvalidInputMessage = "Please ensure the input contains at least one character.";

        /// <summary>The error message to be placed in 
        /// <see cref="EncryptionFormOptions"/> or <see cref="DecryptionFormOptions"/> in the appropriate index
        /// when a submitted key is empty.</summary>
        public const string InvalidKeyMessage = "Please ensure the key contains at least one character.";

        /// <summary>Creates a cipher system that returns the given number of decryption solutions when decrypting. 
        /// Also populates the basic decryption and encryption form <see cref="Option"/>s in <see cref="DecryptionFormOptions"/> and <see cref="EncryptionFormOptions"/>.</summary>
        /// <param name="numSols">The number of solutions that should be returned when decrypting with a system.</param>
        public Cipher(int numSols) 
        {
            NumSolutionsToReturn = numSols > 0 ? numSols : 1;
            Init();
        }
        /// <summary>Creates a cipher system that returns one solution when decrypting. 
        /// Also populates the basic decryption and encryption form <see cref="Option"/>s in <see cref="DecryptionFormOptions"/> and <see cref="EncryptionFormOptions"/>.</summary>
        public Cipher() {
            NumSolutionsToReturn = 1;
            Init();
        }

        /// <summary>Initializes and populates <see cref="DecryptionFormOptions"/> and <see cref="EncryptionFormOptions"/> with basic <see cref="Option"/>s shared by most systems.
        /// Also calls <see cref="GenerateForms"/> to append subclass specific options to <see cref="DecryptionFormOptions"/> and <see cref="EncryptionFormOptions"/>.</summary>
        public void Init() 
        {
            Name = "default";
            EncryptionFormOptions = new List<Option>();
            EncryptionFormOptions.Add(new TextAreaOption("plaintextInput", null, null, "Enter plaintext here..."));
            EncryptionFormOptions.Add(new TextBoxOption("encryptionAlphabet", "Enter alphabet", standardAlphabet));
            DecryptionFormOptions = new List<Option>();
            DecryptionFormOptions.Add(new TextAreaOption("ciphertextInput", null, null, "Enter ciphertext here..."));
            DecryptionFormOptions.Add(new TextBoxOption("decryptionAlphabet", "Enter alphabet", standardAlphabet));
            GenerateForms();
            outputs = new string[NumSolutionsToReturn];
        }

        /// <summary>Used to initiate encryption verification and execution, if it should occur.</summary>
        public void VerifyAndEncrypt()
        {
            if (EncryptionFormDataIsValid()) Encrypt();
        }

        /// <summary>Used to initiate decryption verification and execution, if it should occur.</summary>
        public void VerifyAndDecrypt()
        {
            if (DecryptionFormDataIsValid()) Decrypt();
        }

        /// <summary>Resets the error message for every <see cref="Option"/> in <see cref="EncryptionFormOptions"/> and <see cref="DecryptionFormOptions"/>.</summary>
        public void ResetErrors()
        {
            foreach (Option opt in EncryptionFormOptions)
            {
                opt.ErrorMessage = null;
            }
            foreach (Option opt in DecryptionFormOptions)
            {
                opt.ErrorMessage = null;
            }
        }

        /// <summary>Performs the least positive residue modulus operation, or throws an error if said operation cannot be performed.</summary>
        /// <param name="n">The modulee.</param>
        /// <param name="mod">The modulus.</param>
        /// <returns>Returns a value in the range [0, <paramref name="mod"/>-1] in true modulo fashion.</returns>
        public int Mod(int n, int mod) 
        {
            if (mod < 1) throw new Exception("Modulus must be > 0.");
            return n > -1 ? n % mod : ((n % mod) + mod) % mod;
        }

        /// <summary>Determines if a given alphabet is valid.</summary>
        /// <param name="alphabet">The alphabet to validate.</param>
        /// <returns>True if the given alphabet contains at least one character, false otherwise.</returns>
        protected bool IsValidAlphabet(string alphabet) 
        {
            return !(alphabet == "" || alphabet == null);
        }

        /// <summary>Determines if a given text input is valid.</summary>
        /// <param name="textInput">The text input to validate.</param>
        /// <returns>True if the given text input contains at least one character, false otherwise.</returns>
        protected bool IsValidTextInput(string textInput)
        {
            return !(textInput == "" || textInput == null);
        }

        /// <summary>Determines if two alphabets are equivalent.</summary>
        /// <param name="alph1">The first alphabet to compare.</param>
        /// <param name="alph2">The second alphabet to compare.</param>
        /// <returns>True if alphabets contain the same information and are of the same length, false otherwise.</returns>
        protected bool AlphabetsMatch(string alph1, string alph2)
        {
            return alph1 == alph2;
        }

        /// <summary>Determines if two alphabets have the same size.</summary>
        /// <param name="alph1">The first alphabet to compare.</param>
        /// <param name="alph2">The second alphabet to compare.</param>
        /// <returns>True if alphabets are the same length, false otherwise.</returns>
        protected bool AlphabetSizeMatch(string alph1, string alph2)
        {
            return alph1.Length == alph2.Length;
        }

        /// <summary>Determines if an alphabet contains every character of a key.</summary>
        /// <param name="alphabet">The alphabet to use in comparison.</param>
        /// <param name="key">The key to use in comparison.</param>
        /// <returns>True if the alphabet contains every character that the key does, false otherwise.</returns>
        protected bool AlphabetContainsKey(string alphabet, string key) 
        {
            foreach (char c in key) 
            {
                if (alphabet.IndexOf(c) == -1) return false;
            }
            return true;
        }

        /// <summary>Generates an error message to be placed in the appropriate <see cref="Option"/>'s <see cref="Option.ErrorMessage"/>
        /// in <see cref="DecryptionFormOptions"/> or <see cref="EncryptionFormOptions"/> when the base and substitution alphabet's lengths aren't equal.</summary>
        /// <param name="baseA">The base alphabet.</param>
        /// <param name="substitution">The substitution alphabet.</param>
        /// <returns>An error message displaying the size discrepancy of the base and substitution alphabets.</returns>
        protected string FormatUnequalAlphabetMessage(string baseA, string substitution) {
            return String.Format("Please ensure alphabets contain the same number of characters (Base: {0} characters, Substitution: {1} characters)", baseA.Length, substitution.Length);
        }
    }
}
