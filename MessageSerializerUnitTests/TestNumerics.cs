using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestNumericsMessage : IMessageSerializable
    {
        public byte Byte { get; set; }
        public short Short { get; set; }
        public ushort UShort { get; set; }
        public int Int { get; set; }
        public uint UInt { get; set; }
        public long Long { get; set; }
        public ulong ULong { get; set; }

        //[MessageSerializedProperty(Length = 10)]
        //public ushort UShortAsLength10 { get; set; }

        //[MessageSerializedProperty(Length = 2)]
        //public ulong ULongAsLength2 { get; set; }
    }

    [TestFixture]
    public class TestNumerics : MessageUnitTestBase<TestNumericsMessage>
    {
        [Test]
        public void Test()
        {
            TestNumericsMessage testMessage = new TestNumericsMessage();
            testMessage.Byte = 0x01;
            testMessage.Short = -30000;
            testMessage.UShort = 35000;
            testMessage.Int = -1879048192;
            testMessage.UInt = 3000000000;
            testMessage.Long = -4000000000;
            testMessage.ULong = 5000000000;
            //testMessage.ULongAsLength2 = 1234;
            //testMessage.UShortAsLength10 = 65530;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                //Assert.That(bytes.Length, Is.EqualTo(41));
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Byte), "Byte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Short", serialized.Short);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShort", serialized.UShort);
                byteIndex += CheckNumeric(bytes, byteIndex, "Int", serialized.Int);
                byteIndex += CheckNumeric(bytes, byteIndex, "UInt", serialized.UInt);
                byteIndex += CheckNumeric(bytes, byteIndex, "Long", serialized.Long);
                byteIndex += CheckNumeric(bytes, byteIndex, "ULong", serialized.ULong);
                //byteIndex += CheckNumeric(bytes, byteIndex, "ULongAsLength2", serialized.ULongAsLength2);
                //byteIndex += CheckNumeric(bytes, byteIndex, "UShortAsLength10", serialized.UShortAsLength10);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Byte, Is.EqualTo(originalObject.Byte), "Byte");
                Assert.That(deserializedObject.Short, Is.EqualTo(originalObject.Short), "Short");
                Assert.That(deserializedObject.UShort, Is.EqualTo(originalObject.UShort), "UShort");
                Assert.That(deserializedObject.Int, Is.EqualTo(originalObject.Int), "Int");
                Assert.That(deserializedObject.UInt, Is.EqualTo(originalObject.UInt), "UInt");
                Assert.That(deserializedObject.Long, Is.EqualTo(originalObject.Long), "Long");
                Assert.That(deserializedObject.ULong, Is.EqualTo(originalObject.ULong), "ULong");
                //Assert.That(deserializedObject.ULongAsLength2, Is.EqualTo(originalObject.ULongAsLength2), "ULongAsLength2");
                //Assert.That(deserializedObject.UShortAsLength10, Is.EqualTo(originalObject.UShortAsLength10), "UShortAsLength10");
            });
        }
    }
}
