using System.Linq;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class CalculatorTestChecksum5555 : CalculatorBase<ushort>
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

            return (ushort)(checksum ^ 0x5555);
        }
    }

    public class CalculatedChecksum5555ResultAttributeDefaults : CalculatedFieldResultAttributeDefaults
    {
        public CalculatedChecksum5555ResultAttributeDefaults()
        {
            Name = "Checksum5555";
            Calculator = typeof(CalculatorTestChecksum5555);
            DefaultStart = Position.NextField;
            DefaultEnd = Position.EndOfMessage;
            Priority = 3000;
            Verify = true;
        }
    }

    public class CalculatedChecksum5555Attribute : CalculatedFieldAttribute
    {
        public CalculatedChecksum5555Attribute()
            : base(new CalculatedChecksum5555ResultAttributeDefaults())
        {
        }
    }

    public class CalculatedChecksum5555ResultAttribute : CalculatedFieldResultAttribute
    {
        public CalculatedChecksum5555ResultAttribute()
            : base(new CalculatedChecksum5555ResultAttributeDefaults())
        {
        }
    }

    public class PropertyRuleTestChecksum5555 : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (!messageSerializedPropertyInfo.CalculatedFieldAttributes.Any(item => item is CalculatedChecksum5555ResultAttribute || item is CalculatedChecksum5555Attribute))
            {
                if (messageSerializedPropertyInfo.PropertyInfo.Name == "Checksum")
                {
                    messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedChecksum5555ResultAttribute());
                }

                if (messageSerializedPropertyInfo.PropertyInfo.Name == "NumberNotIncludedInChecksum")
                {
                    CalculatedChecksum5555Attribute attribute = new CalculatedChecksum5555Attribute();
                    attribute.Exclude = true;
                    messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(attribute);
                }
            }
        }
    }

    public interface ITestChecksum5555Message : IMessageSerializable
    {
        ushort Checksum { get; set; }
        byte MessageType { get; set; }
        byte Length { get; set; }
        uint SomeNumber { get; set; }
        ushort NumberNotIncludedInChecksum { get; set; }
        uint SomeOtherNumber { get; set; }
    }

    public class TestChecksum5555MessageWithNewAttributes : ITestChecksum5555Message
    {
        [CalculatedChecksum5555Result]
        public ushort Checksum { get; set; }
        public byte MessageType { get; set; }
        public byte Length { get; set; }
        public uint SomeNumber { get; set; }
        [CalculatedChecksum5555(Exclude = true)]
        public ushort NumberNotIncludedInChecksum { get; set; }
        public uint SomeOtherNumber { get; set; }
    }

    public class TestChecksum5555MessageWithOldAttributes : ITestChecksum5555Message
    {
        [CalculatedFieldResult(Name = "ChecksumOldAttribute", Calculator = typeof(CalculatorTestChecksum5555), Priority = 3000, DefaultStart = Position.NextField, DefaultEnd = Position.EndOfMessage)]
        public ushort Checksum { get; set; }
        public byte MessageType { get; set; }
        public byte Length { get; set; }
        public uint SomeNumber { get; set; }
        [CalculatedField(Name = "ChecksumOldAttribute", Exclude = true)]
        public ushort NumberNotIncludedInChecksum { get; set; }
        public uint SomeOtherNumber { get; set; }
    }

    public class TestChecksum5555MessageWithNoAttributes : ITestChecksum5555Message
    {
        public ushort Checksum { get; set; }
        public byte MessageType { get; set; }
        public byte Length { get; set; }
        public uint SomeNumber { get; set; }
        public ushort NumberNotIncludedInChecksum { get; set; }
        public uint SomeOtherNumber { get; set; }
    }

    [TestFixture]
    public class TestSampleCalculatedFieldsUserDefined : MessageUnitTestBase<ITestChecksum5555Message>
    {
        protected void RunTest<T>(SerializationDefaults serializationDefaults) where T : class, ITestChecksum5555Message, new()
        {
            var testMessage = new T();
            testMessage.MessageType = 1;
            testMessage.SomeNumber = 2;
            testMessage.NumberNotIncludedInChecksum = 3;
            testMessage.SomeOtherNumber = 4;

            // Length will automatically be calculated as 10
            // Checksum is total of bytes in MessageType, Length, SomeNumber and SomeOtherNumber so 1 + 10 + 2 + 4 = 17
            // 17 = 0x0011 and 0x0011 ^ 0x5555 = 0x5544 so that is what our CalculatorTestChecksum5555 will return

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(14), "Total Length");
                Assert.That(serialized.Length, Is.EqualTo(10), "Length Byte");
                Assert.That(serialized.Checksum, Is.EqualTo(0x5544), "Calculated Checksum");
                byteIndex += CheckNumeric(bytes, byteIndex, "Checksum", serialized.Checksum);
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageType), "MessageType");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Length), "Length");
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeNumber", serialized.SomeNumber);
                byteIndex += CheckNumeric(bytes, byteIndex, "NumberNotIncludedInChecksum", serialized.NumberNotIncludedInChecksum);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherNumber", serialized.SomeOtherNumber);
            }, serializationDefaults);

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Checksum, Is.EqualTo(originalObject.Checksum), "Checksum");
                Assert.That(deserializedObject.MessageType, Is.EqualTo(originalObject.MessageType), "MessageType");
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.SomeNumber, Is.EqualTo(originalObject.SomeNumber), "SomeNumber");
                Assert.That(deserializedObject.NumberNotIncludedInChecksum, Is.EqualTo(originalObject.NumberNotIncludedInChecksum), "NumberNotIncludedInChecksum");
                Assert.That(deserializedObject.SomeOtherNumber, Is.EqualTo(originalObject.SomeOtherNumber), "SomeOtherNumber");
            }, serializationDefaults);
        }

        [Test]
        public void TestNewAttributes()
        {
            RunTest<TestChecksum5555MessageWithNewAttributes>(null);
        }

        [Test]
        public void TestOldAttributes()
        {
            RunTest<TestChecksum5555MessageWithOldAttributes>(null);
        }

        [Test]
        public void TestNoAttributes()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.PropertyRules.Add(new PropertyRuleTestChecksum5555());
            RunTest<TestChecksum5555MessageWithNoAttributes>(serializationDefaults);
        }
    }
}
