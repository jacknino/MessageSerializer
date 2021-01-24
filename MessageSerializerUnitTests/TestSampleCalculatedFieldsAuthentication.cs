using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class CalculatorTestChecksum : CalculatorBase<ushort>
    {
        public override ushort Calculate(params byte[][] arrays)
        {
            ushort checksum = 0;
            foreach (byte[] field in arrays)
            {
                foreach (byte currentByte in field)
                {
                    checksum += currentByte;
                }
            }

            return checksum;
        }
    }

    public class TestCalculatedAuthenticationMessage : IMessageSerializable
    {
        public byte MessageType { get; set; }
        public byte Length { get; set; }
        [CalculatedAuthentication(Start = Position.ThisField)]
        public ushort FirstFieldIncludedInChecksum { get; set; }
        public uint SomeNumber { get; set; }
        [CalculatedAuthentication(Exclude = true)]
        public ushort NumberNotIncludedInChecksum { get; set; }
        public uint SomeOtherNumber { get; set; }
        [CalculatedAuthenticationResult(Calculator = typeof(CalculatorTestChecksum))]
        public ushort Checksum { get; set; }
    }

    [TestFixture]
    public class TestSampleCalculatedFieldsAuthentication : MessageUnitTestBase<TestCalculatedAuthenticationMessage>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestCalculatedAuthenticationMessage();
            testMessage.MessageType = 3;
            testMessage.FirstFieldIncludedInChecksum = 1;
            testMessage.SomeNumber = 2;
            testMessage.NumberNotIncludedInChecksum = 3;
            testMessage.SomeOtherNumber = 4;

            // Expected checksum is total of bytes in FirstFieldIncludedInChecksum, SomeNumber and SomeOtherNumber so 1 + 2 + 4 = 7

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(16), "Total Length");
                Assert.That(serialized.Length, Is.EqualTo(14), "Length Byte");
                Assert.That(serialized.Checksum, Is.EqualTo(7), "Calculated Checksum");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageType), "MessageType");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Length), "Length");
                byteIndex += CheckNumeric(bytes, byteIndex, "FirstFieldIncludedInChecksum", serialized.FirstFieldIncludedInChecksum);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeNumber", serialized.SomeNumber);
                byteIndex += CheckNumeric(bytes, byteIndex, "NumberNotIncludedInChecksum", serialized.NumberNotIncludedInChecksum);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherNumber", serialized.SomeOtherNumber);
                byteIndex += CheckNumeric(bytes, byteIndex, "Checksum", serialized.Checksum);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MessageType, Is.EqualTo(originalObject.MessageType), "MessageType");
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.FirstFieldIncludedInChecksum, Is.EqualTo(originalObject.FirstFieldIncludedInChecksum), "FirstFieldIncludedInChecksum");
                Assert.That(deserializedObject.SomeNumber, Is.EqualTo(originalObject.SomeNumber), "SomeNumber");
                Assert.That(deserializedObject.NumberNotIncludedInChecksum, Is.EqualTo(originalObject.NumberNotIncludedInChecksum), "NumberNotIncludedInChecksum");
                Assert.That(deserializedObject.SomeOtherNumber, Is.EqualTo(originalObject.SomeOtherNumber), "SomeOtherNumber");
                Assert.That(deserializedObject.Checksum, Is.EqualTo(originalObject.Checksum), "Checksum");
            });
        }
    }
}
