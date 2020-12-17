using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestAuthenticationCrcClasses : TestAuthenticationBase
    {
        // This site does calculations online: https://crccalc.com/
        // For hex values you just enter them as one big string with no delimiters or 0x
        private readonly byte[] _testInputData = new byte[]
        {
            0x12, 0x23, 0x45, 0x47, 0x48, 0x53, 0xAF, 0x23, 0x45, 0x32, 0x98, 0xBA, 0x9A, 0xC0, 0x22, 0x33, 0x44, 0x55, 0x66, 0xAA, 0xBC, 0xCD, 0xDE, 0xEF
        };

        private const ushort ExpectedCrc16Result = 0x79B6;
        private const uint ExpectedCrc32Result = 0x995E5452;

        [Test]
        public void TestCrc16()
        {
            RunTestWithTestInputData<CalculatorAuthenticationCrc16, ushort>(_testInputData, ExpectedCrc16Result);
        }

        [Test]
        public void TestCrc32()
        {
            RunTestWithTestInputData<CalculatorAuthenticationCrc32, uint>(_testInputData, ExpectedCrc32Result);
        }
    }
}
