using NUnit.Framework;
using Project_EFT.Data_Classes;
using Project_EFT.Ciphers;
using Project_EFT.Ciphers.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Project_EFT_Tests
{
    class VigenereTest
    {

        Vigenere v1, v2, v3, v4;

        [SetUp]
        public void Setup()
        {
            v1 = new Vigenere(1);
            v2 = new Vigenere(1);
            v3 = new Vigenere(1);
            v4 = new Vigenere(1);


            //encryption small alphabet cipher
            v1.EncryptionFormOptions[Cipher.InputIndex].SetValue("a");
            v1.EncryptionFormOptions[Cipher.AlphabetIndex].SetValue("a");
            v1.EncryptionFormOptions[Vigenere.KeyIndex].SetValue("a");

            //encryption normal alphabet cipher
            v2.EncryptionFormOptions[Cipher.InputIndex].SetValue("this is a test");
            v2.EncryptionFormOptions[Cipher.AlphabetIndex].SetValue("abcdefghijklmnopqrstuvwxyz");
            v2.EncryptionFormOptions[Vigenere.KeyIndex].SetValue("cat");

            //decryption on small alphabet
            v3.DecryptionFormOptions[Cipher.InputIndex].SetValue("vhbu il c txut");
            v3.DecryptionFormOptions[Cipher.AlphabetIndex].SetValue("abcdefghijklmnopqrstuvwxyz");
            ((RadioOptionsSet)v3.DecryptionFormOptions[Vigenere.DecryptionMethodIndex]).Choices[Vigenere.FullKeyDecryptionChoice].SetValue("cat");

            //decryption on normal alphabet
            v4.DecryptionFormOptions[Cipher.InputIndex].SetValue("tiiv sw uhbt kktneos zril ypu hxgmdf srwirhjnj. sx ahbnjow gt't asziyrbnfo, eld zox rete oo lniy wiaw iss osijsrylmy hxgmdfd.");
            v4.DecryptionFormOptions[Cipher.AlphabetIndex].SetValue("abcdefghijklmnopqrstuvwxyz");
            ((RadioOptionsSet)v4.DecryptionFormOptions[Vigenere.DecryptionMethodIndex]).Choices[Vigenere.FullKeyDecryptionChoice].SetValue("abadkey");


        }

        [Test]
        public void testEncrypt()
        {
            v1.VerifyAndEncrypt();
            Assert.True((string)v1.DecryptionFormOptions[Cipher.InputIndex].GetValue() == "a");

            v2.VerifyAndEncrypt();
            Console.WriteLine((string)v2.DecryptionFormOptions[Cipher.InputIndex].GetValue());
            Assert.True((string)v2.DecryptionFormOptions[Cipher.InputIndex].GetValue() == "vhbu il c txut");

        }

        [Test]
        public void testDecrypt()
        {
            v3.VerifyAndDecrypt();
            Assert.True((string)v3.EncryptionFormOptions[Cipher.InputIndex].GetValue() == "this is a test");

            v4.VerifyAndDecrypt();
            Assert.True((string)v4.EncryptionFormOptions[Cipher.InputIndex].GetValue() == "this is what happens when you encode something. it changes it's appearance, and you have no idea what you originally encoded.");
        }
    }
}
