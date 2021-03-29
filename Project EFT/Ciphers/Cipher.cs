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
        public abstract string Encrypt();
        public abstract string[] Decrypt();
        public const int InputIndex = 0;
        public const int AlphabetIndex = 1;

        public Cipher(int numSols) 
        {
            NumSolutionsToReturn = numSols;
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
            EncryptionFormOptions.Add(new TextBoxOption("encryptionAlphabet", "Enter alphabet", "abcdefghijklmnopqrstuvwxyz"));
            DecryptionFormOptions = new List<Option>();
            DecryptionFormOptions.Add(new TextAreaOption("ciphertextInput"));
            DecryptionFormOptions.Add(new TextBoxOption("decryptionAlphabet", "Enter alphabet", "abcdefghijklmnopqrstuvwxyz"));
            GenerateForms();
            outputs = new string[NumSolutionsToReturn];
        }

        public int Mod(int n, int mod) 
        {
            return n > -1 ? n % mod : ((n % mod) + mod) % mod;
        }

    }
}
