using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestAuthenticationsMessageCrc32 : IMessageSerializable
    {
        public byte Byte { get; set; }
        [CalculatedAuthentication(Exclude = true)]
        public short Short { get; set; }
        public ushort UShort { get; set; }

        [CalculatedAuthenticationResult(typeof(CalculatorAuthenticationCrc32))]
        public uint Crc { get; set; }
    }

    public class TestAuthenticationsMessageSha256 : IMessageSerializable
    {
        public byte Byte { get; set; }
        [CalculatedAuthentication(Exclude = true)]
        public short Short { get; set; }
        public ushort UShort { get; set; }

        // https://emn178.github.io/online-tools/sha256.html
        [MessageProperty(Length = 32)]
        [CalculatedAuthenticationResult(typeof(CalculatorAuthenticationSha256))]
        public byte[] Sha { get; set; }
    }

    public class TestAuthenticationsMessageSha256WithLength : IMessageSerializable
    {
        public byte Byte { get; set; }
        [CalculatedAuthentication(Exclude = true)]
        public short Short { get; set; }
        public ushort UShort { get; set; }

        // What we are testing here is because Sha is a fixed length we should be able to include the length in the authentication calculation
        // But that ends up being a special case compared to other fields
        public byte Length { get; set; }
        [MessageProperty(Length = 32)]
        [CalculatedLength(Exclude = false)]
        [CalculatedAuthenticationResult(typeof(CalculatorAuthenticationSha256))]
        public byte[] Sha { get; set; }
    }

    [TestFixture]
    public class TestAuthenticationsCrc32 : MessageUnitTestBase<TestAuthenticationsMessageCrc32>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestAuthenticationsMessageCrc32();
            testMessage.Byte = 0x01;
            testMessage.Short = -30000;
            testMessage.UShort = 35000;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Byte), "Byte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Short", serialized.Short);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShort", serialized.UShort);
                byteIndex += CheckNumeric(bytes, byteIndex, "Crc", serialized.Crc);
                // Online CRC calculator: https://crccalc.com/
                Assert.That(serialized.Crc, Is.EqualTo(0x31FC9C87), "CrcValue");
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Byte, Is.EqualTo(originalObject.Byte), "Byte");
                Assert.That(deserializedObject.Short, Is.EqualTo(originalObject.Short), "Short");
                Assert.That(deserializedObject.UShort, Is.EqualTo(originalObject.UShort), "UShort");
                Assert.That(deserializedObject.Crc, Is.EqualTo(originalObject.Crc), "Crc");
            });
        }
    }

    [TestFixture]
    public class TestAuthenticationsSha256 : MessageUnitTestBase<TestAuthenticationsMessageSha256>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestAuthenticationsMessageSha256();
            testMessage.Byte = 0x01;
            testMessage.Short = -30000;
            testMessage.UShort = 35000;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Byte), "Byte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Short", serialized.Short);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShort", serialized.UShort);
                //byteIndex += CheckNumeric(bytes, byteIndex, "Sha", serialized.Sha);
                // 7fbb10e796995e1d650e10377f997cb9ff52167889cfe6a73603f29e67bea797
                byte[] expectedSha = new byte[] {0xe0, 0x09, 0x14, 0x5a, 0x9f, 0xbd, 0x48, 0x0d, 0x72, 0x2c, 0x47, 0x7d, 0x19, 0x18, 0x60, 0x6b, 0x46, 0xa5, 0xee, 0x5a, 0x1d, 0x19, 0x8d, 0x8d, 0x1a, 0x31, 0x75, 0x82, 0x4a, 0x93, 0x47, 0x4f};
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Sha", expectedSha);
                https://emn178.github.io/online-tools/sha256.html
                Assert.That(serialized.Sha, Is.EqualTo(expectedSha), "ShaValue");
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Byte, Is.EqualTo(originalObject.Byte), "Byte");
                Assert.That(deserializedObject.Short, Is.EqualTo(originalObject.Short), "Short");
                Assert.That(deserializedObject.UShort, Is.EqualTo(originalObject.UShort), "UShort");
                Assert.That(deserializedObject.Sha, Is.EqualTo(originalObject.Sha), "Sha");
            });
        }
    }

    [TestFixture]
    public class TestAuthenticationsSha256WithLength : MessageUnitTestBase<TestAuthenticationsMessageSha256WithLength>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestAuthenticationsMessageSha256WithLength();
            testMessage.Byte = 0x01;
            testMessage.Short = -30000;
            testMessage.UShort = 35000;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Byte), "Byte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Short", serialized.Short);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShort", serialized.UShort);
                //byteIndex += CheckNumeric(bytes, byteIndex, "Sha", serialized.Sha);
                byteIndex += CheckNumeric(bytes, byteIndex, "Length", serialized.Length);
                byte[] expectedSha = new byte[] {0xf1, 0x2c, 0xc5, 0x9e, 0xc4, 0x00, 0x76, 0xc4, 0x87, 0xb0, 0x46, 0x3c, 0xaa, 0x7d, 0x2f, 0x6b, 0x14, 0x3f, 0x6b, 0x4e, 0x86, 0xaf, 0xfa, 0x8c, 0xe0, 0x92, 0x1b, 0x0e, 0xf2, 0xe5, 0x20, 0x97};
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Sha", expectedSha);
                https://emn178.github.io/online-tools/sha256.html
                Assert.That(serialized.Sha, Is.EqualTo(expectedSha), "ShaValue");
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Byte, Is.EqualTo(originalObject.Byte), "Byte");
                Assert.That(deserializedObject.Short, Is.EqualTo(originalObject.Short), "Short");
                Assert.That(deserializedObject.UShort, Is.EqualTo(originalObject.UShort), "UShort");
                Assert.That(deserializedObject.Sha, Is.EqualTo(originalObject.Sha), "Sha");
            });
        }
    }
}
