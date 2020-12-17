using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestEndianness
    {
        public enum TestEnum : int
        {
            Low = 0x01,
            High = Int32.MaxValue - 2 // 0x7FFFFFFD
        }

        public interface ITestClass
        {
            byte Byte { get; set; }
            short Short { get; set; }
            int Int { get; set; }
            TestEnum Enum { get; set; }
            byte Length { get; set; }
            List<short> ListShort { get; set; }
        }

        public class TestClassNoDecoration : ITestClass, IMessageSerializable
        {
            public byte Byte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public TestEnum Enum { get; set; }
            public byte Length { get; set; }
            public List<short> ListShort { get; set; }
        }

        [MessageClass(Endianness = Endiannesses.Big)]
        public class TestClassClassAttributeBigEndian : ITestClass, IMessageSerializable
        {
            public byte Byte { get; set; }
            public short Short { get; set; }
            public int Int { get; set; }
            public TestEnum Enum { get; set; }
            public byte Length { get; set; }
            public List<short> ListShort { get; set; }
        }

        [MessageClass(Endianness = Endiannesses.Big)]
        public class TestClassClassAttributeBigEndianWithIndividualLittle : ITestClass, IMessageSerializable
        {
            public byte Byte { get; set; }
            public short Short { get; set; }
            [MessageProperty(Endianness = Endiannesses.Little)]
            public int Int { get; set; }
            public TestEnum Enum { get; set; }
            public byte Length { get; set; }
            [MessageProperty(Endianness = Endiannesses.Little)]
            public List<short> ListShort { get; set; }
        }

        protected void TestEndiannessClass<TTestClass>(SerializationDefaults serializationDefaults, byte[] expectedSerialization)
            where TTestClass : class, ITestClass, IMessageSerializable, new()
        {
            TTestClass testClass = new TTestClass();
            testClass.Byte = 0x01;
            testClass.Short = 0x0203;
            testClass.Int = 0x04050607;
            testClass.Enum = TestEnum.High;
            testClass.ListShort = new List<short>();
            testClass.ListShort.Add(0x1122);
            testClass.ListShort.Add(0x3344);

            Serializer.Instance.GetClassInfo(typeof(TTestClass), true, serializationDefaults);

            byte[] serialized = Serializer.Instance.Serialize(testClass);
            Assert.That(testClass.Length, Is.EqualTo(4), "Length");
            Assert.That(serialized, Is.EqualTo(expectedSerialization), "Serialize");

            TTestClass testClassDeserialized = Serializer.Instance.Deserialize<TTestClass>(serialized);
            Assert.That(testClassDeserialized.Byte, Is.EqualTo(testClass.Byte), "Byte");
            Assert.That(testClassDeserialized.Short, Is.EqualTo(testClass.Short), "Short");
            Assert.That(testClassDeserialized.Int, Is.EqualTo(testClass.Int), "Int");
        }

        [Test]
        public void TestDefault()
        {
            TestEndiannessClass<TestClassNoDecoration>(new SerializationDefaults(), new byte[] { 0x01, 0x03, 0x02, 0x07, 0x06, 0x05, 0x04, 0xFD, 0xFF, 0xFF, 0x7F, 0x04, 0x22, 0x11, 0x44, 0x33 });
        }

        [Test]
        public void TestNoDecorationWithDefaultLittleEndian()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.Endianness = Endiannesses.Little;
            TestEndiannessClass<TestClassNoDecoration>(serializationDefaults, new byte[] { 0x01, 0x03, 0x02, 0x07, 0x06, 0x05, 0x04, 0xFD, 0xFF, 0xFF, 0x7F, 0x04, 0x22, 0x11, 0x44, 0x33 });
        }

        [Test]
        public void TestNoDecorationWithDefaultBigEndian()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.Endianness = Endiannesses.Big;
            TestEndiannessClass<TestClassNoDecoration>(serializationDefaults, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x7F, 0xFF, 0xFF, 0xFD, 0x04, 0x11, 0x22, 0x33, 0x44 });
        }

        [Test]
        public void TestClassAttributeBigEndianWithDefaultLittleEndian()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.Endianness = Endiannesses.Little;
            TestEndiannessClass<TestClassClassAttributeBigEndian>(serializationDefaults, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x7F, 0xFF, 0xFF, 0xFD, 0x04, 0x11, 0x22, 0x33, 0x44 });
        }

        [Test]
        public void TestClassAttributeBigEndianWithDefaultLittleEndianAndIndividualLittle()
        {
            SerializationDefaults serializationDefaults = new SerializationDefaults();
            serializationDefaults.Endianness = Endiannesses.Little;
            TestEndiannessClass<TestClassClassAttributeBigEndianWithIndividualLittle>(serializationDefaults, new byte[] { 0x01, 0x02, 0x03, 0x07, 0x06, 0x05, 0x04, 0x7F, 0xFF, 0xFF, 0xFD, 0x04, 0x22, 0x11, 0x44, 0x33 });
        }
    }
}
