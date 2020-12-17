using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestStringsMessage : IMessageSerializable
    {
        public int Length { get; set; }

        [MessageProperty(Length = 6)]
        public string FixedShort { get; set; }

        [MessageProperty(Length = 6)]
        public string FixedEqual { get; set; }

        [MessageProperty(Length = 6)]
        public string FixedLong { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte VariableMinLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)] // In theory want to set MinLength = 3 but not supported yet
        public string VariableMin { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte VariableMaxLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)] // In theory want to set MaxLength = 6 but not supported yet
        public string VariableMax { get; set; }

        public string VariableField { get; set; }
    }

    [TestFixture]
    public class TestStrings : MessageUnitTestBase<TestStringsMessage>
    {
        [Test]
        public void Test()
        {
            TestStringsMessage testMessage = new TestStringsMessage();
            testMessage.FixedShort = "333";
            testMessage.FixedEqual = "666666";
            testMessage.FixedLong = "999999999";
            testMessage.VariableMin = "1";
            testMessage.VariableMax = "88888888";
            testMessage.VariableField = "1234567890";

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                byteIndex += CheckNumeric(bytes, byteIndex, "Length", serialized.Length);
                byteIndex += CheckStringMatches(bytes, byteIndex, "FixedShort", "333", 6);
                byteIndex += CheckStringMatches(bytes, byteIndex, "FixedEqual", "666666");
                byteIndex += CheckStringMatches(bytes, byteIndex, "FixedLong", "999999");
                Assert.That(serialized.VariableMinLength, Is.EqualTo(1), "MinLength");
                byteIndex += CheckNumeric(bytes, byteIndex, "VariableMinLength", serialized.VariableMinLength);
                byteIndex += CheckStringMatches(bytes, byteIndex, "VariableMinLength", "1");
                Assert.That(serialized.VariableMaxLength, Is.EqualTo(8), "MaxLength");
                byteIndex += CheckNumeric(bytes, byteIndex, "VariableMaxLength", serialized.VariableMaxLength);
                byteIndex += CheckStringMatches(bytes, byteIndex, "VariableMinLength", "88888888");
                byteIndex += CheckStringMatches(bytes, byteIndex, "VariableField", "1234567890");
                Assert.That(bytes.Length, Is.EqualTo(43));
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.FixedShort, Is.EqualTo(originalObject.FixedShort), "FixedShort");
                Assert.That(deserializedObject.FixedEqual, Is.EqualTo(originalObject.FixedEqual), "FixedEqual");
                Assert.That(deserializedObject.FixedLong, Is.EqualTo(originalObject.FixedLong.Substring(0, 6)), "FixedLong");
                Assert.That(deserializedObject.VariableMinLength, Is.EqualTo(originalObject.VariableMinLength), "VariableMinLength");
                Assert.That(deserializedObject.VariableMin, Is.EqualTo(originalObject.VariableMin), "VariableMin");
                Assert.That(deserializedObject.VariableMaxLength, Is.EqualTo(originalObject.VariableMaxLength), "VariableMaxLength");
                Assert.That(deserializedObject.VariableMax, Is.EqualTo(originalObject.VariableMax), "VariableMax");
                Assert.That(deserializedObject.VariableField, Is.EqualTo(originalObject.VariableField), "VariableField");
            });
        }
    }
}
