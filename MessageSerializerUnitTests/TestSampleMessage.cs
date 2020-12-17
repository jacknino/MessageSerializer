using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class SampleMessage : IMessageSerializable
    {
        public byte MessageType { get; set; }
        public byte Length { get; set; }
        public ushort Id { get; set; }
    }

    [TestFixture]
    public class TestSampleMessage : MessageUnitTestBase<SampleMessage>
    {
        [Test]
        public void Test()
        {
            SampleMessage sampleMessage = new SampleMessage();
            sampleMessage.MessageType = 0x23;
            sampleMessage.Id = 0x1234;

            byte[] sampleMessageSerialized = Serializer.Instance.Serialize(sampleMessage);
            SampleMessage sampleMessageDeserialized = Serializer.Instance.Deserialize<SampleMessage>(sampleMessageSerialized);

            byte[] serializedBytes = TestSerialize(sampleMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(4), "Total Length");
                Assert.That(serialized.Length, Is.EqualTo(2), "Length Byte");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageType), "MessageType");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Length), "Length");
                byteIndex += CheckNumeric(bytes, byteIndex, "Id", serialized.Id);
            });

            TestDeserialize(serializedBytes, sampleMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MessageType, Is.EqualTo(originalObject.MessageType), "MessageType");
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.Id, Is.EqualTo(originalObject.Id), "Id");
            });
        }
    }
}
