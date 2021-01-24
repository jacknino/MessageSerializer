using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestCalculatedLengthMessage : IMessageSerializable
    {
        public byte MessageType { get; set; }
        [CalculatedLengthResult]
        public byte MessageLength { get; set; }
        [CalculatedLength(Exclude = true)]
        public ushort SomeRandomFieldNotIncludedInLength { get; set; }
        public uint SomeNumber { get; set; }
        public uint SomeOtherNumber { get; set; }
        public string SomeStringOfVaryingLength { get; set; }
        [CalculatedLength(End = Position.PreviousField)]
        public byte SomeOtherFieldNotIncludedInTheLength { get; set; }
    }

    [TestFixture]
    public class TestSampleCalculatedFieldsLength : MessageUnitTestBase<TestCalculatedLengthMessage>
    {
        [Test]
        public void Test()
        {
            var testMessage = new TestCalculatedLengthMessage();
            testMessage.MessageType = 3;
            testMessage.SomeRandomFieldNotIncludedInLength = 1;
            testMessage.SomeNumber = 2;
            testMessage.SomeOtherNumber = 3;
            testMessage.SomeStringOfVaryingLength = "ThisString";
            testMessage.SomeOtherFieldNotIncludedInTheLength = 4;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(23), "Total Length");
                Assert.That(serialized.MessageLength, Is.EqualTo(18), "Length Byte");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageType), "MessageType");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageLength), "MessageLength");
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeRandomFieldNotIncludedInLength", serialized.SomeRandomFieldNotIncludedInLength);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeNumber", serialized.SomeNumber);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherNumber", serialized.SomeOtherNumber);
                byteIndex += CheckStringMatches(bytes, byteIndex, "SomeStringOfVaryingLength", serialized.SomeStringOfVaryingLength);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherFieldNotIncludedInTheLength", serialized.SomeOtherFieldNotIncludedInTheLength);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MessageType, Is.EqualTo(originalObject.MessageType), "MessageType");
                Assert.That(deserializedObject.MessageLength, Is.EqualTo(originalObject.MessageLength), "MessageLength");
                Assert.That(deserializedObject.SomeRandomFieldNotIncludedInLength, Is.EqualTo(originalObject.SomeRandomFieldNotIncludedInLength), "SomeRandomFieldNotIncludedInLength");
                Assert.That(deserializedObject.SomeNumber, Is.EqualTo(originalObject.SomeNumber), "SomeNumber");
                Assert.That(deserializedObject.SomeOtherNumber, Is.EqualTo(originalObject.SomeOtherNumber), "SomeOtherNumber");
                Assert.That(deserializedObject.SomeStringOfVaryingLength, Is.EqualTo(originalObject.SomeStringOfVaryingLength), "SomeStringOfVaryingLength");
                Assert.That(deserializedObject.SomeOtherFieldNotIncludedInTheLength, Is.EqualTo(originalObject.SomeOtherFieldNotIncludedInTheLength), "SomeOtherFieldNotIncludedInTheLength");
            });
        }
    }
}
