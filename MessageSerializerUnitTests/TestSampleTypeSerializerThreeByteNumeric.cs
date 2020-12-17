using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestSampleTypeSerializerThreeByteNumeric : TestTypeSerializerBase
    {
        public class TypeSerializerThreeByteNumeric : TypeSerializerBase<int>
        {
            public TypeSerializerThreeByteNumeric(MessageSerializedPropertyInfo propertyInfo)
                : base(propertyInfo)
            {
            }

            public override byte[] Serialize(int value)
            {
                // We want to cut off the most significant byte so we get the array as little endian
                // then cut off the last byte (the MSB) and then determine if we need to reverse the bytes
                byte[] returnArray = ArrayOps.GetBytesFromNumeric(value, Endiannesses.Little);
                if (returnArray[3] != 0)
                    throw new Exception($"For {_propertyInfo.PropertyInfo.Name}, {value} can not be serialized into a 3-byte value");

                return ArrayOps.GetSubArray(returnArray, 0, 3, ArrayOps.EndiannessRequiresReversal(Endiannesses.Little, _propertyInfo.MessagePropertyAttribute.Endianness));
            }

            public override int Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
            {
                // GetNumeric expects there to be the correct number of bytes for the type it is converting to
                // So we need to first get the 3 bytes we want and get them in little endian order
                // This is so when we resize to 4 bytes the 0x00 for the MSB will be correctly put at the end
                // Then when we call GetNumeric we make sure to indicate that the bytes are currently in little endian order
                byte[] workingArray = ArrayOps.GetSubArray(bytes, currentArrayIndex, 3, ArrayOps.EndiannessRequiresReversal(_propertyInfo.MessagePropertyAttribute.Endianness, Endiannesses.Little));
                Array.Resize(ref workingArray, 4);
                int value = ArrayOps.GetNumeric<int>(workingArray, Endiannesses.Little);
                currentArrayIndex += 3;
                return value;
            }
        }

        public class TestClass : IMessageSerializable
        {
            [MessageProperty(TypeSerializerClass = typeof(TypeSerializerThreeByteNumeric), Endianness = Endiannesses.System)]
            public int System { get; set; }

            [MessageProperty(TypeSerializerClass = typeof(TypeSerializerThreeByteNumeric), Endianness = Endiannesses.Little)]
            public int Little { get; set; }

            [MessageProperty(TypeSerializerClass = typeof(TypeSerializerThreeByteNumeric), Endianness = Endiannesses.Big)]
            public int Big { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestSampleTypeSerializerThreeByteNumeric()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, int valueToUse, byte[] expectedArray)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerThreeByteNumeric typeSerializer = new TypeSerializerThreeByteNumeric(propertyInfo);
            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        [Test]
        public void Test()
        {
            bool systemIsLittleEndian = ArrayOps.SystemEndiannessIsLittleEndian();
            TestField(_classInfo, "System", 3, systemIsLittleEndian ? new byte[] { 0x03, 0x00, 0x00 } : new byte[] { 0x00, 0x00, 0x03 });
            TestField(_classInfo, "Little", 0x123456, new byte[] { 0x56, 0x34, 0x12 });
            TestField(_classInfo, "Big", 0x123456, new byte[] { 0x12, 0x34, 0x56 });
        }
    }
}
