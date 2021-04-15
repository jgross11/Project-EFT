using NUnit.Framework;
using Project_EFT.Data_Classes;
using Project_EFT.Ciphers;
using Project_EFT.Ciphers.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Project_EFT_Tests
{
    class CaesarCipherTest
    {

        CaesarCipher cs1, cs2, og, cs3, cs4;

        [SetUp]
        public void Setup()
        {
            cs1 = new CaesarCipher(1);
            cs2 = new CaesarCipher(1);
            og = new CaesarCipher(1);
            cs3 = new CaesarCipher(1);
            cs4 = new CaesarCipher(1);

            //encryption small alphabet cipher
            cs1.EncryptionFormOptions[0].SetValue("a");
            cs1.EncryptionFormOptions[1].SetValue("a");
            cs1.EncryptionFormOptions[2].SetValue("10");

            //encryption bigger alphabet
            cs2.EncryptionFormOptions[0].SetValue("abc");
            cs2.EncryptionFormOptions[1].SetValue("abc");
            cs2.EncryptionFormOptions[2].SetValue("1");


            //encryption normal alphabet
            og.EncryptionFormOptions[0].SetValue("this is a test");
            og.EncryptionFormOptions[1].SetValue("abcdefghijklmnopqrstuvwxyz");
            og.EncryptionFormOptions[2].SetValue("3");

            //decryption on small alphabet
            cs3.DecryptionFormOptions[0].SetValue("dcba");
            cs3.DecryptionFormOptions[1].SetValue("abcd");
            cs3.DecryptionFormOptions[2].SetValue("0");
            ((RadioOptionsSet)cs3.DecryptionFormOptions[2]).Choices[0].SetValue("7");

            //decryption on a normal alphabet
            cs4.DecryptionFormOptions[0].SetValue("wklv lv d whvw");
            cs4.DecryptionFormOptions[1].SetValue("abcdefghijklmnopqrstuvwxyz");
            cs4.DecryptionFormOptions[2].SetValue("0");
            ((RadioOptionsSet)cs4.DecryptionFormOptions[2]).Choices[0].SetValue("3");
        }

        [Test]
        public void testEncrypt()
        {
            // test encryption on small alphabet
            cs1.VerifyAndEncrypt();
            Assert.True((string)cs1.DecryptionFormOptions[0].GetValue() == "a");

            //test encryption on medium alphabet
            cs2.VerifyAndEncrypt();
            Assert.True((string)cs2.DecryptionFormOptions[0].GetValue() == "bca");

            //test encryption normally
            og.VerifyAndEncrypt();
            Assert.True((string)og.DecryptionFormOptions[0].GetValue() == "wklv lv d whvw");
        }

        [Test]
        public void testDecrypt()
        {
            cs3.VerifyAndDecrypt();
            Assert.True((string)cs3.EncryptionFormOptions[0].GetValue() == "adcb");

            cs4.VerifyAndDecrypt();
            Assert.True((string)cs4.EncryptionFormOptions[0].GetValue() == "this is a test");

        }
    }
}
