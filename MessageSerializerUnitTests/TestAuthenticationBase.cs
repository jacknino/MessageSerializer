using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestAuthenticationBase
    {
        protected byte[][] GetDataInChunks(byte[] data)
        {
            Random random = new Random();

            List<byte[]> byteArrays = new List<byte[]>();
            int currentIndex = 0;
            // We'll get chunks of from 1 to 8 bytes
            while (currentIndex < data.Length)
            {
                int bytesRemaining = data.Length - currentIndex;
                int chunkLength = Math.Min((random.Next() % 8) + 1, bytesRemaining);
                byte[] nextChunk = ArrayOps.GetSubArray(data, currentIndex, chunkLength);
                byteArrays.Add(nextChunk);
                currentIndex += chunkLength;
            }

            return byteArrays.ToArray();
        }

        protected void VerifyResults<TClassToTest, TReturnType>(TClassToTest testClass, TReturnType expectedCrc, TReturnType computedCrc) where TClassToTest : CalculatorAuthenticationBase<TReturnType>
        {
            DeserializeStatus status = new DeserializeStatus();
            Assert.That(testClass.Verify(status, expectedCrc, computedCrc), Is.True);
            Assert.That(computedCrc, Is.EqualTo(expectedCrc));
        }

        protected void RunTestWithTestInputData<TClassToTest, TReturnType>(byte[] testInputData, TReturnType expectedCrc) where TClassToTest : CalculatorAuthenticationBase<TReturnType>, new()
        {
            TClassToTest crcClassToTest = new TClassToTest();
            TReturnType computedCrc = crcClassToTest.Calculate(testInputData);
            VerifyResults(crcClassToTest, expectedCrc, computedCrc);

            TReturnType staticComputedCrc = CalculatorAuthenticationBase<TReturnType>.Compute<TClassToTest>(testInputData);
            Assert.That(staticComputedCrc, Is.EqualTo(expectedCrc));
            Assert.That(CalculatorAuthenticationBase<TReturnType>.Verify<TClassToTest>(expectedCrc, staticComputedCrc));

            // Now we want to test with broken up arrays
            byte[][] separatedInput = GetDataInChunks(testInputData);
            TReturnType separateComputedCrc = crcClassToTest.Calculate(separatedInput);
            VerifyResults(crcClassToTest, expectedCrc, separateComputedCrc);
        }
    }
}
