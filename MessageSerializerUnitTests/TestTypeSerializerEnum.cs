using System;
using System.Runtime.InteropServices;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerEnum : TestTypeSerializerBase
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

        public class TestClass
        {
            public ByteEnum ByteEnum { get; set; }
            public ShortEnum ShortEnum { get; set; }
            public UShortEnum UShortEnum { get; set; }
            public IntEnum IntEnum { get; set; }
            public LongEnum LongEnum { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerEnum()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected byte[] GetExpectedByteArray<TEnumType>(TEnumType value) where TEnumType : struct, IConvertible
        {
            Type underlyingType = typeof(TEnumType).GetEnumUnderlyingType();

            if (Marshal.SizeOf(underlyingType) == 1)
                return new byte[] { (byte)(object)value };

            var getBytesMethod = typeof(BitConverter).GetMethod("GetBytes", new[] { underlyingType });
            if (getBytesMethod == null)
                throw new Exception($"Couldn't get BitConverter.GetBytes method for type {typeof(TEnumType).FullName}");

            var result = getBytesMethod.Invoke(null, new object[] { value });
            return (byte[])result;
        }

        protected void TestField<TEnumType>(MessageSerializedClassInfo classInfo, string propertyName, TEnumType valueToUse) where TEnumType : struct, IConvertible
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerEnum<TEnumType> typeSerializer = new TypeSerializerEnum<TEnumType>(propertyInfo);
            byte[] expectedArray = GetExpectedByteArray(valueToUse);

            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected void TestFieldWithValues<TEnumType>(MessageSerializedClassInfo classInfo, string propertyName, TEnumType[] valuesToUse) where TEnumType : struct, IConvertible
        {
            foreach (TEnumType value in valuesToUse)
            {
                TestField(classInfo, propertyName, value);
            }
        }

        [Test]
        public void TestByteEnum()
        {
            TestFieldWithValues(_classInfo, "ByteEnum", new [] { ByteEnum.Value1, ByteEnum.ValueMax });
        }

        [Test]
        public void TestShortEnum()
        {
            TestFieldWithValues(_classInfo, "ShortEnum", new [] { ShortEnum.Value1, ShortEnum.ValueMax });
        }

        [Test]
        public void TestUShortEnum()
        {
            TestFieldWithValues(_classInfo, "UShortEnum", new [] { UShortEnum.Value1, UShortEnum.ValueMax });
        }

        [Test]
        public void TestIntEnum()
        {
            TestFieldWithValues(_classInfo, "IntEnum", new[] { IntEnum.Value1, IntEnum.ValueMax });
        }

        [Test]
        public void TestLongEnum()
        {
            TestFieldWithValues(_classInfo, "LongEnum", new[] { LongEnum.Value1, LongEnum.ValueMax });
        }
    }
}
