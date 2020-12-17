using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerBcd : TestTypeSerializerBase
    {
        public class TestClass : IMessageSerializable
        {
            public byte BcdByte { get; set; } // Should be length 2 (to fit 255)
            public ushort BcdUShort { get; set; } // Should be length 3 (to fit 65,535)
            public uint BcdUInt { get; set; } // Should be length 5 (to fit 4,294,967,295)
            public ulong BcdULong { get; set; } // Should be length 10 (to fit 18,446,744,073,709,551,615)

            [MessageProperty(Length = 10)]
            public ushort BcdUShortAsLength10 { get; set; }

            [MessageProperty(Length = 2)]
            public ulong BcdULongAsLength2 { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerBcd()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected void TestField<TNumericType>(MessageSerializedClassInfo classInfo, string propertyName, TNumericType valueToUse, byte[] expectedArray) 
            where TNumericType : struct, IComparable<TNumericType>, IConvertible
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerBcd<TNumericType> typeSerializer = new TypeSerializerBcd<TNumericType>(propertyInfo);

            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected override void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            string expectedValueString = $"{expectedValue}";
            if (expectedValueString.Length > (valueArray.Length * 2))
            {
                expectedValueString = expectedValueString.Remove(0, expectedValueString.Length - (valueArray.Length * 2));
                expectedValue = ArrayOps.Parse<TValueType>(expectedValueString);
            }

            Assert.That(deserializedValue, Is.EqualTo(expectedValue), "Deserialize");
        }

        protected void TestFieldWithValues<TNumericType>(MessageSerializedClassInfo classInfo, string propertyName, TNumericType[] valuesToUse, byte[][] expectedByteArrays)             where TNumericType : struct, IComparable<TNumericType>, IConvertible
        {
            Assert.That(valuesToUse.Length, Is.EqualTo(expectedByteArrays.Length), "TestValuesLength");
            for (int index = 0; index < valuesToUse.Length; ++index)
            {
                TestField(classInfo, propertyName, valuesToUse[index], expectedByteArrays[index]);
            }
        }

        [Test]
        public void TestBcdByte()
        {
            TestFieldWithValues(_classInfo, "BcdByte", new byte[] { 123, 1, 254 }, new byte[][] { new byte[] {0x01, 0x23}, new byte[] { 0x00, 0x01 }, new byte[] { 0x02, 0x54 }});
        }

        [Test]
        public void TestBcdUShort()
        {
            TestFieldWithValues(_classInfo, "BcdUShort", new ushort[] { 123, 65100 }, new byte[][] { new byte[] { 0x00, 0x01, 0x23 }, new byte[] { 0x06, 0x51, 0x00 } });
        }

        [Test]
        public void TestBcdUInt()
        {
            TestFieldWithValues(_classInfo, "BcdUInt", new uint[] { 123, 12345, 4223456789 }, new byte[][] {
                new byte[] { 0x00, 0x00, 0x00, 0x01, 0x23 },
                new byte[] { 0x00, 0x00, 0x01, 0x23, 0x45 },
                new byte[] { 0x42, 0x23, 0x45, 0x67, 0x89 }
            });
        }

        [Test]
        public void TestBcdULong()
        {
            TestFieldWithValues(_classInfo, "BcdULong", new ulong[] { 123, 12345, 123456789, 9223372036854775807, 18223372036854775807 }, new byte[][] {
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 },
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23, 0x45 },
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23, 0x45, 0x67, 0x89 },
                new byte[] { 0x09, 0x22, 0x33, 0x72, 0x03, 0x68, 0x54, 0x77, 0x58, 0x07 },
                new byte[] { 0x18, 0x22, 0x33, 0x72, 0x03, 0x68, 0x54, 0x77, 0x58, 0x07 }
            });
        }

        [Test]
        public void TestBcdUShortAsLength10()
        {
            TestFieldWithValues(_classInfo, "BcdUShortAsLength10", new ushort[] { 123, 65100 }, new byte[][] {
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x23 },
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x51, 0x00 }
            });
        }

        [Test]
        public void TestBcdULongAsLength2()
        {
            TestFieldWithValues(_classInfo, "BcdULongAsLength2", new ulong[] { 123, 12345, 123456789, 9223372036854775807, 18223372036854775807 }, new byte[][] {
                new byte[] { 0x01, 0x23 },
                new byte[] { 0x23, 0x45 },
                new byte[] { 0x67, 0x89 },
                new byte[] { 0x58, 0x07 },
                new byte[] { 0x58, 0x07 }
            });
        }
    }
}
