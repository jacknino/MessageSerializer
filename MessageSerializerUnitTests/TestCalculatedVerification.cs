using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestCalculatedVerification
    {
        public class TestMessage : IMessageSerializable
        {
            [CalculatedLengthResult(Verify = true)]
            public int Length { get; set; }
            public int SomeInt { get; set; }
            public ushort Crc { get; set; }
        }

        [Test]
        public void Test()
        {
            TestMessage testMessage = new TestMessage();
            testMessage.SomeInt = 3;

            ushort expectedCrc = 0x94B1;

            byte[] serializedBytes = Serializer.Instance.Serialize(testMessage);
            Assert.That(testMessage.Length, Is.EqualTo(4), "Length Serialized");
            Assert.That(testMessage.Crc, Is.EqualTo(expectedCrc), "Crc Serialized");

            DeserializeResults<TestMessage> deserializeResults = Serializer.Instance.DeserializeEx<TestMessage>(serializedBytes, true);
            TestMessage deserializedMessage = deserializeResults.Object;
            Assert.That(deserializedMessage.Length, Is.EqualTo(testMessage.Length), "Deserialized Length");
            Assert.That(deserializedMessage.SomeInt, Is.EqualTo(testMessage.SomeInt), "Deserialized SomeInt");
            Assert.That(deserializedMessage.Crc, Is.EqualTo(testMessage.Crc), "Deserialized Crc");
            Assert.That(deserializeResults.Status.Results, Is.True, "Deserialized status");

            // Now we screw up the CRC and we should get a failed result
            byte[] badCrc = new byte[serializedBytes.Length];
            Array.Copy(serializedBytes, badCrc, serializedBytes.Length);
            badCrc[badCrc.Length - 1] = 0x00;
            DeserializeResults<TestMessage> deserializeResultsBadCrc = Serializer.Instance.DeserializeEx<TestMessage>(badCrc, true);
            Assert.That(deserializeResultsBadCrc.Status.Results, Is.False, "BadDeserialized status");
            TestMessage deserializedMessageBadCrc = deserializeResultsBadCrc.Object;
            Assert.That(deserializedMessageBadCrc.Length, Is.EqualTo(testMessage.Length), "BadDeserialized Length");
            Assert.That(deserializedMessageBadCrc.SomeInt, Is.EqualTo(testMessage.SomeInt), "BadDeserialized SomeInt");
            Assert.That(deserializedMessageBadCrc.Crc, Is.EqualTo(expectedCrc & 0x00FF), "BadDeserialized Crc");
        }
    }
}
