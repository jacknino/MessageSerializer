using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerNumeric : TestTypeSerializerBase
    {
        public class TestClass
        {
            public byte Byte { get; set; }
            public sbyte SByte { get; set; }
            public short Short { get; set; }
            public ushort UShort { get; set; }
            public int Int { get; set; }
            public uint UInt { get; set; }
            public long Long { get; set; }
            public ulong ULong { get; set; }
            public int Length { get; set; }
            public List<uint> ListUInt { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerNumeric()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected byte[] GetExpectedByteArray<TNumericType>(TNumericType value) where TNumericType : struct
        {
            return ArrayOps.GetBytes(value);
        }

        protected void TestField<TNumericType>(MessageSerializedClassInfo classInfo, string propertyName, TNumericType valueToUse) where TNumericType : struct
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerNumeric<TNumericType> typeSerializer = new TypeSerializerNumeric<TNumericType>(propertyInfo);
            byte[] expectedArray = GetExpectedByteArray(valueToUse);
            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected void TestListField<TNumericType>(MessageSerializedClassInfo classInfo, string propertyName, List<TNumericType> valueToUse) where TNumericType : struct
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerNumeric<TNumericType> typeSerializer = new TypeSerializerNumeric<TNumericType>(propertyInfo);
            byte[] expectedArray = GetExpectedByteArray(valueToUse);
            TestListField<TypeSerializerNumeric<TNumericType>, List<TNumericType>, TNumericType>(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected byte[] GetExpectedByteArray<TNumericType>(List<TNumericType> list) where TNumericType : struct
        {
            byte[] expectedByteArray = new byte[0];
            foreach (TNumericType item in list)
            {
                expectedByteArray = ArrayOps.Combine(expectedByteArray, GetExpectedByteArray(item));
            }

            return expectedByteArray;
        }

        protected override string GetExpectedToStringValueFormat(MessageSerializedPropertyInfo propertyInfo)
        {
            return $"{{0}} (0x{{0:X{propertyInfo.MessagePropertyAttribute.Length * 2}}})";
        }

        protected void TestFieldWithValues<TNumericType>(MessageSerializedClassInfo classInfo, string propertyName, TNumericType[] valuesToUse) where TNumericType : struct
        {
            foreach (TNumericType value in valuesToUse)
            {
                TestField(classInfo, propertyName, value);
            }
        }

        [Test]
        public void TestByte()
        {
            TestFieldWithValues(_classInfo, "Byte", new byte[] { 123, 254, 1 });
        }

        [Test]
        public void TestSByte()
        {
            TestFieldWithValues(_classInfo, "SByte", new sbyte[] { 123, -123 });
        }

        [Test]
        public void TestShort()
        {
            TestFieldWithValues(_classInfo, "Short", new short[] { 123, 12345, -12345 });
        }

        [Test]
        public void TestUShort()
        {
            TestFieldWithValues(_classInfo, "UShort", new ushort[] { 123, 65100 });
        }

        [Test]
        public void TestInt()
        {
            TestFieldWithValues(_classInfo, "Int", new int[] { 123, 12345, -12345, 123456789, -123456789 });
        }

        [Test]
        public void TestUInt()
        {
            TestFieldWithValues(_classInfo, "UInt", new uint[] { 123, 12345, 4223456789 });
        }

        [Test]
        public void TestLong()
        {
            TestFieldWithValues(_classInfo, "Long", new long[] { 123, 12345, -12345, 123456789, -123456789, 9223372036854775807, -9223372036854775807 });
        }

        [Test]
        public void TestULong()
        {
            TestFieldWithValues(_classInfo, "ULong", new ulong[] { 123, 12345, 123456789, 9223372036854775807, 18223372036854775807 });
        }

        [Test]
        public void TestListUInt()
        {
            List<uint> listToUse = new List<uint> { 123, 12345, 123456789 };

            TestListField(_classInfo, "ListUInt", listToUse);
        }
    }
}
