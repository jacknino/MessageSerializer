using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerByteArray : TestTypeSerializerBase
    {
        protected byte[][] _testArrays;

        public class TestClass
        {
            [MessageProperty(BlobType = BlobTypes.Length)]
            public int NoRestrictionsLength { get; set; }

            [MessageProperty(BlobType = BlobTypes.Data)]
            public byte[] NoRestrictions { get; set; }

            [MessageProperty(Length = 6)]
            public byte[] FixedLength6 { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int MinLength3Length { get; set; }

            [MessageProperty(MinLength = 3, BlobType = BlobTypes.Data)]
            public byte[] MinLength3 { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int MaxLength10Length { get; set; }

            [MessageProperty(MaxLength = 10, BlobType = BlobTypes.Data)]
            public byte[] MaxLength10 { get; set; }

            [MessageProperty(BlobType = BlobTypes.Length)]
            public int MinLength3MaxLength10Length { get; set; }

            [MessageProperty(MinLength = 3, MaxLength = 10, BlobType = BlobTypes.Data)]
            public byte[] MinLength3MaxLength10 { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerByteArray()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
            _testArrays = new byte[][]
            {
                new byte[] { 0x01 },
                new byte[] { 0x01, 0x02, 0x03, 0x04 },
                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 },
                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 },
                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B },
                new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F },
            };
        }

        protected int CalculateOutputLength(MessageSerializedPropertyInfo propertyInfo, byte[] value)
        {
            int outputLength = value.Length;
            if (propertyInfo.MessagePropertyAttribute.VariableLength)
            {
                int minLength = propertyInfo.MessagePropertyAttribute.MinLength;
                int maxLength = propertyInfo.MessagePropertyAttribute.MaxLength;
                if (minLength > 0 && minLength > outputLength)
                    outputLength = minLength;

                if (maxLength > 0 && maxLength < outputLength)
                    outputLength = maxLength;
            }
            else
            {
                outputLength = propertyInfo.MessagePropertyAttribute.Length;
            }

            return outputLength;
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, byte[] valueToUse, int expectedOutputLength)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerByteArray typeSerializer = new TypeSerializerByteArray(propertyInfo);

            //int outputLength = CalculateOutputLength(propertyInfo, valueToUse);
            byte[] expectedArray = (byte[])valueToUse.Clone();
            Array.Resize(ref expectedArray, expectedOutputLength);

            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected override void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            byte[] deserializedValueArray = (byte[])(object)deserializedValue;

            // In our case the valueArray is what we actually expect things to be since it will already be resized to the proper value
            base.VerifyDeserialized(deserializedValueArray, valueArray, propertyInfo, valueArray);
        }

        protected void TestFieldWithValues(MessageSerializedClassInfo classInfo, string propertyName, byte[][] valuesToUse, int[] expectedOutputLengths)
        {
            for (int index = 0; index < valuesToUse.Length; ++index)
            {
                TestField(classInfo, propertyName, valuesToUse[index], expectedOutputLengths[index]);
            }
        }

        protected override void CheckToString<TTypeSerializer, TValueType>(TTypeSerializer typeSerializer, MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray) 
        {
            base.CheckToString(typeSerializer, propertyInfo, (TValueType)(object)expectedArray, expectedArray);
        }

        protected override string GetToStringValue<TValueType>(MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray)
        {
            return BitConverter.ToString(expectedArray);
        }

        [Test]
        public void TestNoRestrictions()
        {
            TestFieldWithValues(_classInfo, "NoRestrictions", _testArrays, new int[] { 1, 4, 6, 9, 11, 15 });
        }

        [Test]
        public void TestFixedLength6()
        {
            TestFieldWithValues(_classInfo, "FixedLength6", _testArrays, new int[] { 6, 6, 6, 6, 6, 6 });
        }

        [Test]
        public void TestMinLength3()
        {
            TestFieldWithValues(_classInfo, "MinLength3", _testArrays, new int[] { 3, 4, 6, 9, 11, 15 });
        }

        [Test]
        public void TestMaxLength10()
        {
            TestFieldWithValues(_classInfo, "MaxLength10", _testArrays, new int[] { 1, 4, 6, 9, 10, 10 });
        }

        [Test]
        public void TestMinLength3MaxLength10()
        {
            TestFieldWithValues(_classInfo, "MinLength3MaxLength10", _testArrays, new int[] { 3, 4, 6, 9, 10, 10 });
        }
    }
}
