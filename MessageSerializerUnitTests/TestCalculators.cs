using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestCalculatorsMessageCrc32 : IMessageSerializable
    {
        [CalculatedLengthResult]
        public byte Length { get; set; }
        public byte Byte { get; set; }
        //[MessageProperty(ExcludeFromAuthentication = true)]
        [CalculatedAuthentication(Exclude = true)]
        public short Short { get; set; }
        public ushort UShort { get; set; }
        //[MessageProperty(ExcludeFromAuthentication = true)]
        [CalculatedAuthentication(Exclude = true)]
        public string String { get; set; }

        //[MessageProperty(AuthenticationClass = typeof(AuthenticationCrc32))]
        [CalculatedAuthenticationResult(typeof(CalculatorAuthenticationCrc32))]
        [CalculatedLength(Exclude = true)]
        public uint BlahCrc { get; set; }
    }

    [TestFixture]
    public class TestCalculators : MessageUnitTestBase<TestCalculatorsMessageCrc32>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestCalculatorsMessageCrc32();
            testMessage.Byte = 0x01;
            testMessage.Short = -30000;
            testMessage.UShort = 35000;
            testMessage.String = "String";

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(serialized.Length, Is.EqualTo(11), "LengthValue");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Length), "Length");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Byte), "Byte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Short", serialized.Short);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShort", serialized.UShort);
                byteIndex += CheckStringMatches(bytes, byteIndex, "String", testMessage.String);
                byteIndex += CheckNumeric(bytes, byteIndex, "Crc", serialized.BlahCrc);
                // Bytes should look like: 0B01D08AB888String (with String actually being the hex)
                // But crc is only calculated on length, byte and ushort so 0B01B888
                // Online CRC calculator: https://crccalc.com/
                Assert.That(serialized.BlahCrc, Is.EqualTo(0x38F81D88), "CrcValue");
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.Byte, Is.EqualTo(originalObject.Byte), "Byte");
                Assert.That(deserializedObject.Short, Is.EqualTo(originalObject.Short), "Short");
                Assert.That(deserializedObject.UShort, Is.EqualTo(originalObject.UShort), "UShort");
                Assert.That(deserializedObject.String, Is.EqualTo(originalObject.String), "String");
                Assert.That(deserializedObject.BlahCrc, Is.EqualTo(originalObject.BlahCrc), "Crc");
            });
        }
    }
}
