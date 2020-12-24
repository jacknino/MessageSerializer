using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestPropertyRuleSampleHash : IMessageSerializable
    {
        public byte Length { get; set; }
        public int Int { get; set; }
        public byte[] Hash { get; set; }
    }

    [TestFixture]
    public class TestSamplePropertyRule : MessageUnitTestBase<TestPropertyRuleSampleHash>
    {
        public class PropertyRuleSampleHash : IPropertyRule
        {
            public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
            {
                if (!messageSerializedPropertyInfo.ContainsAuthenticationAttribute
                    && messageSerializedPropertyInfo.PropertyInfo.Name.Equals("Hash", StringComparison.InvariantCultureIgnoreCase))
                {
                    messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationSha256)));

                    // Since we know the result of the hash is going to be 32 bytes if the Length property hasn't been set we'll set it.
                    if (!messageSerializedPropertyInfo.MessagePropertyAttribute.IsLengthSpecified)
                        messageSerializedPropertyInfo.MessagePropertyAttribute.Length = 32;
                }
            }
        }

        [Test]
        public void TestSample()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.PropertyRules.Add(new PropertyRuleSampleHash());

            Serializer.Instance.GetClassInfo(typeof(TestPropertyRuleSampleHash), serializationDefaults);

            TestPropertyRuleSampleHash testMessage = new TestPropertyRuleSampleHash();
            testMessage.Int = 0x01020304;

            byte[] serialized = Serializer.Instance.Serialize(testMessage);
            TestPropertyRuleSampleHash deserialized = Serializer.Instance.Deserialize<TestPropertyRuleSampleHash>(serialized);
        }

        [Test]
        public void Test()
        {
            TestPropertyRuleSampleHash testMessage = new TestPropertyRuleSampleHash();
            testMessage.Int = 0x01020304;

            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.PropertyRules.Add(new PropertyRuleSampleHash());

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(37));
                Assert.That(bytes[byteIndex], Is.EqualTo(36), "LengthByte");
                byteIndex += CheckNumeric(bytes, byteIndex, "Length", serialized.Length);
                byteIndex += CheckNumeric(bytes, byteIndex, "Int", serialized.Int);
                // https://emn178.github.io/online-tools/sha256.html
                byte[] expectedHash = new byte[]
                {
                    0x24, 0x5c, 0x3e, 0x13, 0x49, 0xb2, 0xe4, 0x95,
                    0x71, 0xd2, 0x54, 0x4d, 0xc8, 0xbe, 0x38, 0x61,
                    0xb3, 0xf7, 0xb4, 0x91, 0x57, 0x42, 0x22, 0xae,
                    0x3c, 0xdc, 0x9f, 0x34, 0x52, 0x8c, 0x58, 0xd0
                };
                CheckMultiByteArray(serialized.Hash, 0, "HashSerialized", expectedHash);
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Hash", serialized.Hash);
            }, serializationDefaults);

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.Int, Is.EqualTo(originalObject.Int), "Int");
                Assert.That(deserializedObject.Hash, Is.EqualTo(originalObject.Hash), "Hash");
            }, serializationDefaults);
        }
    }
}
