using System.Text;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestAuthenticationHashClasses : TestAuthenticationBase
    {
        // Test data taken from here: https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.180-4.pdf
        // https://csrc.nist.gov/projects/cryptographic-standards-and-guidelines/example-values
        private static readonly string TestString = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq";
        private static readonly byte[] TestBytes = Encoding.ASCII.GetBytes(TestString);

        [Test]
        public void TestSha1()
        {
            // https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHA1.pdf
            byte[] expectedResult =
            {
                0x84, 0x98, 0x3E, 0x44, 0x1C, 0x3B, 0xD2, 0x6E, 0xBA, 0xAE, 0x4A, 0xA1, 0xF9, 0x51, 0x29, 0xE5, 0xE5, 0x46, 0x70, 0xF1
            };

            RunTestWithTestInputData<CalculatorAuthenticationSha1, byte[]>(TestBytes, expectedResult);
        }

        [Test]
        public void TestSha256()
        {
            // https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHA256.pdf
            byte[] expectedResult =
            {
                0x24, 0x8D, 0x6A, 0x61, 0xD2, 0x06, 0x38, 0xB8, 0xE5, 0xC0, 0x26, 0x93, 0x0C, 0x3E, 0x60, 0x39, 
                0xA3, 0x3C, 0xE4, 0x59, 0x64, 0xFF, 0x21, 0x67, 0xF6, 0xEC, 0xED, 0xD4, 0x19, 0xDB, 0x06, 0xC1 
            };

            RunTestWithTestInputData<CalculatorAuthenticationSha256, byte[]>(TestBytes, expectedResult);
        }

        [Test]
        public void TestSha512()
        {
            // https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/SHA512.pdf
            string testString = "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu";
            byte[] testBytes = Encoding.ASCII.GetBytes(testString);

            byte[] expectedResult =
            {
                0x8E, 0x95, 0x9B, 0x75, 0xDA, 0xE3, 0x13, 0xDA, 0x8C, 0xF4, 0xF7, 0x28, 0x14, 0xFC, 0x14, 0x3F, 
                0x8F, 0x77, 0x79, 0xC6, 0xEB, 0x9F, 0x7F, 0xA1, 0x72, 0x99, 0xAE, 0xAD, 0xB6, 0x88, 0x90, 0x18, 
                0x50, 0x1D, 0x28, 0x9E, 0x49, 0x00, 0xF7, 0xE4, 0x33, 0x1B, 0x99, 0xDE, 0xC4, 0xB5, 0x43, 0x3A, 
                0xC7, 0xD3, 0x29, 0xEE, 0xB6, 0xDD, 0x26, 0x54, 0x5E, 0x96, 0xE5, 0x5B, 0x87, 0x4B, 0xE9, 0x09
            };

            RunTestWithTestInputData<CalculatorAuthenticationSha512, byte[]>(testBytes, expectedResult);
        }
    }
}
