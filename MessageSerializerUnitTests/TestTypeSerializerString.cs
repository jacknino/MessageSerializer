using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerString : TestTypeSerializerBase
    {
        public class TestClass
        {
            public int Length { get; set; }

            [MessageProperty(Length = 6)]
            public string FixedLength6 { get; set; }

            [MessageProperty(Length = 10, PrepadCharacter = 'T')]
            public string FixedLength10PrepadT { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int BlobStringLength { get; set; }

            [MessageProperty(BlobType = BlobTypes.Data)]
            public string BlobString { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int BlobStringLengthMin3Max10 { get; set; }

            [MessageProperty(BlobType = BlobTypes.Data, MinLength = 3, MaxLength = 10)]
            public string BlobStringMin3Max10 { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int BlobStringLengthMin3Max10PrepadC { get; set; }

            [MessageProperty(BlobType = BlobTypes.Data, MinLength = 3, MaxLength = 10, PrepadCharacter = 'C')]
            public string BlobStringMin3Max10PrepadC { get; set; }

            public string VariableString { get; set; }

            // Check MinLength, MaxLength, PadCharacters
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerString()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected int CalculateOutputLength(MessageSerializedPropertyInfo propertyInfo, string value)
        {
            int outputLength = propertyInfo.MessagePropertyAttribute.Length;
            if (propertyInfo.MessagePropertyAttribute.VariableLength)
            {
                outputLength = value.Length;

                int minLength = propertyInfo.MessagePropertyAttribute.MinLength;
                int maxLength = propertyInfo.MessagePropertyAttribute.MaxLength;
                if (minLength > 0 && minLength > outputLength)
                    outputLength = minLength;

                if (maxLength > 0 && maxLength < outputLength)
                    outputLength = maxLength;
            }

            return outputLength;
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, string valueToUse)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerString typeSerializer = new TypeSerializerString(propertyInfo);

            List<byte> list = new List<byte>();
            foreach(char character in valueToUse)
                list.Add((byte)character);

            int outputLength = CalculateOutputLength(propertyInfo, valueToUse);
            if (outputLength < list.Count)
            {
                list.RemoveRange(outputLength, list.Count - outputLength);
            }
            else if (outputLength > list.Count && propertyInfo.MessagePropertyAttribute.Prepad)
            {
                while (outputLength > list.Count)
                {
                    list.Insert(0, (byte)propertyInfo.MessagePropertyAttribute.PrepadCharacter);
                }
            }
            else if (outputLength > list.Count)
            {
                while (outputLength > list.Count)
                {
                    list.Add((byte)0);
                }
            }

            byte[] expectedArray = list.ToArray();
            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected override void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            // If fixed length but our original value is longer than the fixed length then we need to cut the expected value down
            string deserializedValueString = $"{deserializedValue}";
            string expectedValueString = $"{deserializedValue}";

            int outputLength = CalculateOutputLength(propertyInfo, expectedValueString);
            if (outputLength < expectedValueString.Length)
            {
                expectedValueString = expectedValueString.Substring(0, outputLength);
            }
            else if (outputLength > expectedValueString.Length && propertyInfo.MessagePropertyAttribute.Prepad)
            {
                expectedValueString = new string(propertyInfo.MessagePropertyAttribute.PrepadCharacter, outputLength - expectedValueString.Length) + expectedValueString;
            }

            base.VerifyDeserialized(deserializedValueString, expectedValueString, propertyInfo, valueArray);
        }

        protected void TestFieldWithValues(MessageSerializedClassInfo classInfo, string propertyName, string[] valuesToUse)
        {
            foreach (string value in valuesToUse)
            {
                TestField(classInfo, propertyName, value);
            }
        }

        [Test]
        public void TestFixedLength6()
        {
            TestFieldWithValues(_classInfo, "FixedLength6", new string[] { "1", "123456", "12345678" });
        }

        [Test]
        public void TestBlobString()
        {
            TestFieldWithValues(_classInfo, "BlobString", new string[] { "1", "123456", "12345678" });
        }

        [Test]
        public void TestVariableString()
        {
            TestFieldWithValues(_classInfo, "VariableString", new string[] { "1", "123456", "12345678" });
        }

        [Test]
        public void FixedLength10PrepadT()
        {
            TestFieldWithValues(_classInfo, "FixedLength10PrepadT", new string[] { "1", "123456", "12345678", "1234567890", "1234567890123" });
        }

        [Test]
        public void TestBlobStringMin3Max10()
        {
            TestFieldWithValues(_classInfo, "BlobStringMin3Max10", new string[] { "1", "123456", "12345678", "1234567890", "1234567890123" });
        }

        [Test]
        public void TestBlobStringMin3Max10PrepadC()
        {
            TestFieldWithValues(_classInfo, "BlobStringMin3Max10PrepadC", new string[] { "1", "123456", "12345678", "1234567890", "1234567890123" });
        }
    }
}
