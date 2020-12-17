using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public interface ITestLength
    {
        int Length { get; set; }
        int IntField { get; set; }
        int ActualLength { get; set; }
        int IntField2 { get; set; }
        byte ByteField { get; set; }
        short ShortField { get; set; }
        string VariableString { get; set; }
    }

    public class TestLength<T> : MessageUnitTestBase<T> where T : class, IMessageSerializable, ITestLength, new()
    {
        protected void TestMessage(int expectedActualLength)
        {
            T testMessage = new T();
            testMessage.Length = 1234;
            testMessage.IntField = 0x12345678;
            testMessage.IntField2 = 123456;
            testMessage.ByteField = 33;
            testMessage.ShortField = 4455;
            testMessage.VariableString = "NineBytes";

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                byteIndex += CheckNumeric(bytes, byteIndex, "Length", serialized.Length);
                byteIndex += CheckNumeric(bytes, byteIndex, "IntField", serialized.IntField);
                Assert.That(serialized.ActualLength, Is.EqualTo(expectedActualLength), "ActualLengthField");
                byteIndex += CheckNumeric(bytes, byteIndex, "ActualLength", serialized.ActualLength);
                byteIndex += CheckNumeric(bytes, byteIndex, "IntField2", serialized.IntField2);
                byteIndex += CheckNumeric(bytes, byteIndex, "ByteField", serialized.ByteField);
                byteIndex += CheckNumeric(bytes, byteIndex, "ShortField", serialized.ShortField);
                byteIndex += CheckStringMatches(bytes, byteIndex, "VariableString", serialized.VariableString);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.IntField, Is.EqualTo(originalObject.IntField), "IntField");
                Assert.That(deserializedObject.ActualLength, Is.EqualTo(originalObject.ActualLength), "ActualLength");
                Assert.That(deserializedObject.IntField2, Is.EqualTo(originalObject.IntField2), "IntField2");
                Assert.That(deserializedObject.ByteField, Is.EqualTo(originalObject.ByteField), "ByteField");
                Assert.That(deserializedObject.ShortField, Is.EqualTo(originalObject.ShortField), "ShortField");
                Assert.That(deserializedObject.VariableString, Is.EqualTo(originalObject.VariableString), "VariableString");
            });
        }
    }

    public class TestLengthEntireMessageMessage : IMessageSerializable, ITestLength
    {
        // This field isn't actually the length
        //[MessageProperty(MessageLengthType = MessageLengthTypes.None)]
        [CalculatedLength(Exclude = false)]
        public int Length { get; set; }

        public int IntField { get; set; }

        //[MessageProperty(MessageLengthType = MessageLengthTypes.EntireMessage)]
        [CalculatedLengthResult(Start = Position.StartOfMessage)]
        public int ActualLength { get; set; }

        public int IntField2 { get; set; }
        public byte ByteField { get; set; }
        public short ShortField { get; set; }
        public string VariableString { get; set; }
    }

    public class TestLengthEntireMessage : TestLength<TestLengthEntireMessageMessage>
    {
        [Test]
        public void Test()
        {
            TestMessage(28);
        }
    }

    public class TestLengthEntireMessageWithExclusionMessage : IMessageSerializable, ITestLength
    {
        // This field isn't actually the length
        //[MessageProperty(MessageLengthType = MessageLengthTypes.None)]
        // We want to indicate to PropertyRuleLengthField that it shouldn't assume this field
        // is the length field by put a CalculatedLength attribute on it and we indicate not
        // to exclude it because we do actually want it included.
        [CalculatedLength(Exclude = false)]
        public int Length { get; set; }

        public int IntField { get; set; }

        //[MessageProperty(MessageLengthType = MessageLengthTypes.EntireMessage)]
        [CalculatedLengthResult(Start = Position.StartOfMessage)]
        public int ActualLength { get; set; }

        public int IntField2 { get; set; }

        //[MessageProperty(ExcludeFromLength = true)]
        [CalculatedLength(Exclude = true)]
        public byte ByteField { get; set; }
        public short ShortField { get; set; }
        public string VariableString { get; set; }
    }

    public class TestLengthEntireMessageWithExclusion : TestLength<TestLengthEntireMessageWithExclusionMessage>
    {
        [Test]
        public void Test()
        {
            TestMessage(27);
        }
    }

    public class TestLengthRestOfMessageMessage : IMessageSerializable, ITestLength
    {
        // This field isn't actually the length
        //[MessageProperty(MessageLengthType = MessageLengthTypes.None)]
        [CalculatedLength(Exclude = false)]
        public int Length { get; set; }

        public int IntField { get; set; }

        //[MessageProperty(MessageLengthType = MessageLengthTypes.RestOfMessage)]
        [CalculatedLengthResult]
        public int ActualLength { get; set; }

        public int IntField2 { get; set; }

        //[MessageProperty(ExcludeFromLength = true)]
        [CalculatedLength(Exclude = true)]
        public byte ByteField { get; set; }
        public short ShortField { get; set; }
        public string VariableString { get; set; }
    }

    public class TestLengthRestOfMessage : TestLength<TestLengthRestOfMessageMessage>
    {
        [Test]
        public void Test()
        {
            TestMessage(15);
        }
    }

    public class TestLengthRestOfMessageIncludingLengthMessage : IMessageSerializable, ITestLength
    {
        // This field isn't actually the length
        //[MessageProperty(MessageLengthType = MessageLengthTypes.None)]
        [CalculatedLength(Exclude = false)]
        public int Length { get; set; }

        public int IntField { get; set; }

        //[MessageProperty(MessageLengthType = MessageLengthTypes.RestOfMessageIncludingLength)]
        [CalculatedLengthResult(Start = Position.ThisField)]
        public int ActualLength { get; set; }

        public int IntField2 { get; set; }

        //[MessageProperty(ExcludeFromLength = true)]
        [CalculatedLength(Exclude = true)]
        public byte ByteField { get; set; }
        public short ShortField { get; set; }
        public string VariableString { get; set; }
    }

    public class TestLengthRestOfMessageIncludingLength : TestLength<TestLengthRestOfMessageIncludingLengthMessage>
    {
        [Test]
        public void Test()
        {
            TestMessage(19);
        }
    }

}
