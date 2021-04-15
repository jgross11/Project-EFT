using NUnit.Framework;
using Project_EFT.Data_Classes;
using Project_EFT.Ciphers;
using Project_EFT.Ciphers.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Project_EFT_Tests
{
    class MASCTest
    {

        MASC m1, m2, m3, m4;

        [SetUp]
        public void Setup()
        {
            m1 = new MASC(1);
            m2 = new MASC(1);
            m3 = new MASC(1);
            m4 = new MASC(1);


            //encryption small alphabet cipher
            m1.EncryptionFormOptions[0].SetValue("a");
            m1.EncryptionFormOptions[1].SetValue("a");
            m1.EncryptionFormOptions[2].SetValue("b");

            //encryption normal alphabet cipher
            m2.EncryptionFormOptions[0].SetValue("this is a test");
            m2.EncryptionFormOptions[1].SetValue("abcdefghijklmnopqrstuvwxyz");
            m2.EncryptionFormOptions[2].SetValue("qwertyuiopasdfghjklzxcvbnm");

            //decryption on small alphabet
            m3.DecryptionFormOptions[0].SetValue("qwer");
            m3.DecryptionFormOptions[1].SetValue("abcd");
            m3.DecryptionFormOptions[2].SetValue("0");
            ((RadioOptionsSet)m3.DecryptionFormOptions[2]).Choices[0].SetValue("qwer");

            //decryption on normal alphabet
            m4.DecryptionFormOptions[0].SetValue("ziol ol q ztlz");
            m4.DecryptionFormOptions[1].SetValue("abcdefghijklmnopqrstuvwxyz");
            m4.DecryptionFormOptions[2].SetValue("0");
            ((RadioOptionsSet)m4.DecryptionFormOptions[2]).Choices[0].SetValue("qwertyuiopasdfghjklzxcvbnm");


        }

        [Test]
        public void testEncrypt()
        {
            m1.VerifyAndEncrypt();
            Assert.True((string)m1.DecryptionFormOptions[0].GetValue() == "b");

            m2.VerifyAndEncrypt();
            Assert.True((string)m2.DecryptionFormOptions[0].GetValue() == "ziol ol q ztlz");

        }

        [Test]
        public void testDecrypt()
        {
            m3.VerifyAndDecrypt();
            Assert.True((string)m3.EncryptionFormOptions[0].GetValue() == "abcd");

            m4.VerifyAndDecrypt();
            Assert.True((string)m4.EncryptionFormOptions[0].GetValue() == "this is a test");

        }
    }
}
