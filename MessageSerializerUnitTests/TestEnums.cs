using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public enum ByteEnum : byte
    {
        Value1 = 1,
        ValueMax = 255
    }

    public enum ShortEnum : short
    {
        Value1 = 1,
        ValueMax = 0x7FFF
    }

    public enum UShortEnum : ushort
    {
        Value1 = 1,
        ValueMax = 0xFFFF
    }

    public enum IntEnum : int
    {
        Value1 = 1,
        ValueMax = 0x7FFFFFFF
    }

    public enum LongEnum : long
    {
        Value1 = 1,
        ValueMax = 0x7FFFFFFFFFFFFFFF
    }

    public class TestEnumsMessage : IMessageSerializable
    {
        public ByteEnum ByteEnum { get; set; }
        public ShortEnum ShortEnum { get; set; }
        public UShortEnum UShortEnum { get; set; }
        public IntEnum IntEnum { get; set; }
        public LongEnum LongEnum { get; set; }
    }

    [TestFixture]
    public class TestEnums : MessageUnitTestBase<TestEnumsMessage>
    {
        [Test]
        public void Test()
        {
            TestEnumsMessage testMessage = new TestEnumsMessage();
            testMessage.ByteEnum = ByteEnum.Value1;
            testMessage.ShortEnum = ShortEnum.ValueMax;
            testMessage.UShortEnum = UShortEnum.Value1;
            testMessage.IntEnum = IntEnum.ValueMax;
            testMessage.LongEnum = LongEnum.Value1;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(17));
                byteIndex += CheckNumeric(bytes, byteIndex, "ByteEnum", (byte)serialized.ByteEnum);
                byteIndex += CheckNumeric(bytes, byteIndex, "ShortEnum", (short)serialized.ShortEnum);
                byteIndex += CheckNumeric(bytes, byteIndex, "UShortEnum", (ushort)serialized.UShortEnum);
                byteIndex += CheckNumeric(bytes, byteIndex, "IntEnum", (int)serialized.IntEnum);
                byteIndex += CheckNumeric(bytes, byteIndex, "LongEnum", (long)serialized.LongEnum);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.ByteEnum, Is.EqualTo(originalObject.ByteEnum), "ByteEnum");
                Assert.That(deserializedObject.ShortEnum, Is.EqualTo(originalObject.ShortEnum), "ShortEnum");
                Assert.That(deserializedObject.UShortEnum, Is.EqualTo(originalObject.UShortEnum), "UShortEnum");
                Assert.That(deserializedObject.IntEnum, Is.EqualTo(originalObject.IntEnum), "IntEnum");
                Assert.That(deserializedObject.LongEnum, Is.EqualTo(originalObject.LongEnum), "LongEnum");
            });
        }
    }
}
