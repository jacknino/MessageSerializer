using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestSampleMessageIntroduction
    {
        public class SampleMessage : IMessageSerializable
        {
            public byte MessageType { get; set; }
            public byte Length { get; set; }
            public ushort Id { get; set; }
            public string DeviceName { get; set; }
            public ushort Crc { get; set; }
        }

        [Test]
        public void Test()
        {
            var sampleMessage = new SampleMessage();
            sampleMessage.MessageType = 0x23;
            sampleMessage.Id = 0x1234;
            // "Example" = 0x45, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x0A
            sampleMessage.DeviceName = "Example";

            byte[] sampleMessageSerialized = Serializer.Instance.Serialize(sampleMessage);
            SampleMessage sampleMessageDeserialized = Serializer.Instance.Deserialize<SampleMessage>(sampleMessageSerialized);
        }

        [Test]
        public void TestWithToString()
        {
            var sampleMessage = new SampleMessage();
            sampleMessage.MessageType = 0x23;
            sampleMessage.Id = 0x1234;
            sampleMessage.DeviceName = "Example"; // Correlates to 0x45, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x0A

            Console.WriteLine($"SampleMessageBeforeSerialization: {Serializer.Instance.ToString(sampleMessage, false)}");

            byte[] sampleMessageSerialized = Serializer.Instance.Serialize(sampleMessage);

            Console.WriteLine($"SampleMessageSerialized: {BitConverter.ToString(sampleMessageSerialized)}");
            Console.WriteLine($"SampleMessageAfterSerialization: {Serializer.Instance.ToString(sampleMessage, false)}");

            SampleMessage sampleMessageDeserialized = Serializer.Instance.Deserialize<SampleMessage>(sampleMessageSerialized);

            Console.WriteLine($"SampleMessageDeserialized: {Serializer.Instance.ToString(sampleMessageDeserialized, false)}");
        }

    }
}
