using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project_EFT.Ciphers.Options;

namespace Project_EFT.Ciphers
{
    public abstract class Cipher
    {
        public int NumSolutionsToReturn {get; set;}
        public virtual string Name { get; set; }
        public string[] outputs { get; set; }
        public List<Option> EncryptionFormOptions;
        public List<Option> DecryptionFormOptions;
        public abstract void GenerateForms();
        protected abstract void Encrypt();
        protected abstract void Decrypt();
        protected abstract bool EncryptionFormDataIsValid();
        protected abstract bool DecryptionFormDataIsValid();
        public const int InputIndex = 0;
        public const int AlphabetIndex = 1;
        public const string standardAlphabet = "abcdefghijklmnopqrstuvwxyz";
        public const string InvalidAlphabetMessage = "Please ensure the alphabet contains at least one character.";
        public const string InvalidInputMessage = "Please ensure the input contains at least one character.";

        public Cipher(int numSols) 
        {
            NumSolutionsToReturn = numSols > 0 ? numSols : 1;
            Init();
        }

        public Cipher() {
            NumSolutionsToReturn = 1;
            Init();
        }

        public void Init() 
        {
            Name = "default";
            EncryptionFormOptions = new List<Option>();
            EncryptionFormOptions.Add(new TextAreaOption("plaintextInput"));
            EncryptionFormOptions.Add(new TextBoxOption("encryptionAlphabet", "Enter alphabet", standardAlphabet));
            DecryptionFormOptions = new List<Option>();
            DecryptionFormOptions.Add(new TextAreaOption("ciphertextInput"));
            DecryptionFormOptions.Add(new TextBoxOption("decryptionAlphabet", "Enter alphabet", standardAlphabet));
            GenerateForms();
            outputs = new string[NumSolutionsToReturn];
        }

        public void VerifyAndEncrypt()
        {
            if (EncryptionFormDataIsValid()) Encrypt();
        }

        public void VerifyAndDecrypt()
        {
            if (DecryptionFormDataIsValid()) Decrypt();
        }

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

        public int Mod(int n, int mod) 
        {
            return n > -1 ? n % mod : ((n % mod) + mod) % mod;
        }

        protected bool IsValidAlphabet(string alphabet) 
        {
            return !(alphabet == "" || alphabet == null);
        }

        protected bool IsValidTextInput(string textInput)
        {
            return !(textInput == "" || textInput == null);
        }

        protected bool AlphabetsMatch(string alph1, string alph2)
        {
            return alph1 == alph2;
        }

        protected string FormatUnequalAlphabetMessage(string baseA, string substitution) {
            return String.Format("Please ensure alphabets contain the same number of characters (Base: {0} characters, Substitution: {1} characters)", baseA.Length, substitution.Length);
        }
    }
}
