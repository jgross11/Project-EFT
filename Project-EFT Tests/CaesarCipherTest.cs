using NUnit.Framework;
using Project_EFT.Data_Classes;
namespace Project_EFT_Tests
{
    class CaesarCipherTest
    {

        CaesarCipher cs1, cs2, og;

        [SetUp]
        public void Setup()
        {
            cs1 = new CaesarCipher("a", 10, 0, 1);
            cs2 = new CaesarCipher("abc", 1, 1, 1);
            og = new CaesarCipher("abcdefghijklmnopqrstuvwxyz", 3, 3, 1);
        }

        [Test]
        public void testEncrypt()
        {
            Assert.True(cs1.Encrypt("a") == "a");
            Assert.True(cs2.Encrypt("abc") == "bca");
            Assert.True(cs2.Encrypt("abc cba") == "bca acb");
            Assert.True(og.Encrypt("test...") == "whvw...");
        }
    }
}
