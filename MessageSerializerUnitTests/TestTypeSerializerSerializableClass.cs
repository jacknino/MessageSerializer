using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerSerializableClass : TestTypeSerializerBase
    {
        protected string _expectedToStringResult;

        public class TestSubClass : IMessageSerializable
        {
            public class SubClass : IMessageSerializable
            {
                public byte Field1 { get; set; }
                public int Field2 { get; set; }
            }

            public class MetersBlob : IMessageSerializable
            {
                public ushort MeterNumber { get; set; }
                [MessageProperty(BlobType = BlobTypes.Length)]
                public byte MeterLength { get; set; }
                [MessageProperty(BlobType = BlobTypes.Data)]
                public uint BcdMeterValue { get; set; }
            }

            //[MessageProperty(MessageLengthType = MessageLengthTypes.RestOfMessage)]
            public byte Length { get; set; }
            public List<MetersBlob> Meters { get; set; }
            public SubClass SomeSubClass { get; set; }
        }

        public class TestClass : IMessageSerializable
        {
            public TestSubClass SubClass { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerSerializableClass()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, TestSubClass valueToUse, byte[] expectedArray)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerSerializableClass<TestSubClass> typeSerializer = new TypeSerializerSerializableClass<TestSubClass>(propertyInfo);

            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected override void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            // The "Kind" property after deserialization is "Unspecified" so we need to ignore that part
            TestSubClass deserializedSubClass = deserializedValue as TestSubClass;
            TestSubClass expectedSubClass = expectedValue as TestSubClass;
            Assert.That(deserializedSubClass, Is.Not.Null, "DeserializedTestSubClass");
            Assert.That(expectedSubClass, Is.Not.Null, "ExpectedTestSubClass");

            Assert.That(expectedSubClass.Length, Is.Not.EqualTo(0), "Length 0");
            Assert.That(deserializedSubClass.Length, Is.EqualTo(expectedSubClass.Length), "Length");

            Assert.That(deserializedSubClass.Meters.Count, Is.EqualTo(expectedSubClass.Meters.Count), "MetersCount");
            for (int index = 0; index < deserializedSubClass.Meters.Count; ++index)
            {
                TestSubClass.MetersBlob deserializedMeterBlob = deserializedSubClass.Meters[index];
                TestSubClass.MetersBlob expectedMeterBlob = expectedSubClass.Meters[index];

                Assert.That(deserializedMeterBlob.MeterLength, Is.EqualTo(expectedMeterBlob.MeterLength), "MeterLength");
                Assert.That(deserializedMeterBlob.MeterNumber, Is.EqualTo(expectedMeterBlob.MeterNumber), "MeterNumber");
                Assert.That(deserializedMeterBlob.BcdMeterValue, Is.EqualTo(expectedMeterBlob.BcdMeterValue), "BcdMeterValue");
            }

            Assert.That(deserializedSubClass.SomeSubClass.Field1, Is.EqualTo(expectedSubClass.SomeSubClass.Field1), "Field1");
            Assert.That(deserializedSubClass.SomeSubClass.Field2, Is.EqualTo(expectedSubClass.SomeSubClass.Field2), "Field2");
        }

        protected override void CheckToString<TTypeSerializer, TValueType>(TTypeSerializer typeSerializer, MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray)
        {
            string toStringResult = typeSerializer.ToString(valueToUse, 0, ToStringFormatProperties.Default, true);
            Assert.That(toStringResult, Is.EqualTo(_expectedToStringResult), "ToString");
        }

        [Test]
        public void Test()
        {
            TestClass testClass = new TestClass();
            testClass.SubClass = new TestSubClass();
            testClass.SubClass.Meters = new List<TestSubClass.MetersBlob>();
            testClass.SubClass.Meters.Add(new TestSubClass.MetersBlob() { MeterNumber = 0x2222, BcdMeterValue = 123456 });
            testClass.SubClass.Meters.Add(new TestSubClass.MetersBlob() { MeterNumber = 0x01, BcdMeterValue = 2 });
            testClass.SubClass.Meters.Add(new TestSubClass.MetersBlob() { MeterNumber = 0x2345, BcdMeterValue = 1666666666 });

            testClass.SubClass.SomeSubClass = new TestSubClass.SubClass();
            testClass.SubClass.SomeSubClass.Field1 = 3;
            testClass.SubClass.SomeSubClass.Field2 = -12345666;

            byte[] expectedArray =
            {
                29, // Length

                // MeterBlob0
                0x22, 0x22, // MeterNumber
                5, // MeterLength
                0x00, 0x00, 0x12, 0x34, 0x56, // BcdMeterValue

                // MeterBlob1
                0x01, 0x00, // MeterNumber
                5, // MeterLength
                0x00, 0x00, 0x00, 0x00, 0x02, // BcdMeterValue

                // MeterBlob2
                0x45, 0x23, // MeterNumber
                5, // MeterLength
                0x16, 0x66, 0x66, 0x66, 0x66, // BcdMeterValue

                // SomeSubClass
                0x03, // Field1
                0xBE, 0x9E, 0x43, 0xFF // Field2
            };

            _expectedToStringResult = "SubClass: \r\n" +
                "    Length: 29 (0x1D), \r\n" +
                "    Meters: \r\n" +
                "        Index 0: \r\n" +
                "            MeterNumber: 8738 (0x2222), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 123456\r\n" + 
                "        Index 1: \r\n" +
                "            MeterNumber: 1 (0x0001), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 2\r\n" +
                "        Index 2: \r\n" +
                "            MeterNumber: 9029 (0x2345), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 1666666666, \r\n" +
                "    SomeSubClass: \r\n" +
                "        Field1: 3 (0x03), \r\n" +
                "        Field2: -12345666 (0xFF439EBE)";
            TestField(_classInfo, "SubClass", testClass.SubClass, expectedArray);
        }
    }
}
