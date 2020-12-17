using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestBcdNumericsMessage : IMessageSerializable
    {
        public byte BcdByte { get; set; } // Should be length 2 (to fit 255)
        public ushort BcdUShort { get; set; } // Should be length 3 (to fit 65,535)
        public uint BcdUInt { get; set; } // Should be length 5 (to fit 4,294,967,295)
        public ulong BcdULong { get; set; } // Should be length 10 (to fit 18,446,744,073,709,551,615)

        [MessageProperty(Length = 10)]
        public ushort BcdUShortAsLength10 { get; set; }

        [MessageProperty(Length = 2)]
        public ulong BcdULongAsLength2 { get; set; }
    }

    [TestFixture]
    public class TestBcdNumerics : MessageUnitTestBase<TestBcdNumericsMessage>
    {
        [Test]
        public void Test()
        {
            TestBcdNumericsMessage testMessage = new TestBcdNumericsMessage();
            testMessage.BcdByte = 0x01;
            testMessage.BcdUShort = 35003;
            testMessage.BcdUInt = 3000000003;
            testMessage.BcdULong = 5000000005;
            testMessage.BcdUShortAsLength10 = 65530;
            testMessage.BcdULongAsLength2 = 1234;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(32));
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdByte", serialized.BcdByte);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdUShort", serialized.BcdUShort);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdUInt", serialized.BcdUInt);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdULong", serialized.BcdULong);
                byteIndex += CheckNumericBcd(bytes, byteIndex, 10, "BcdUShortAsLength10", serialized.BcdUShortAsLength10);
                byteIndex += CheckNumericBcd(bytes, byteIndex, 2, "BcdULongAsLength2", serialized.BcdULongAsLength2);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.BcdByte, Is.EqualTo(originalObject.BcdByte), "BcdByte");
                Assert.That(deserializedObject.BcdUShort, Is.EqualTo(originalObject.BcdUShort), "BcdUShort");
                Assert.That(deserializedObject.BcdUInt, Is.EqualTo(originalObject.BcdUInt), "BcdUInt");
                Assert.That(deserializedObject.BcdULong, Is.EqualTo(originalObject.BcdULong), "BcdULong");
                Assert.That(deserializedObject.BcdULongAsLength2, Is.EqualTo(originalObject.BcdULongAsLength2), "BcdULongAsLength2");
                Assert.That(deserializedObject.BcdUShortAsLength10, Is.EqualTo(originalObject.BcdUShortAsLength10), "BcdUShortAsLength10");
            });
        }
    }
}
