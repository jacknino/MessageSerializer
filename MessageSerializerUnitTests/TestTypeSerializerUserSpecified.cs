using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerUserSpecified : TestTypeSerializerBase
    {
        public class TypeSerializerJunk : TypeSerializerBase<int>
        {
            private int _lastSerialized;

            public TypeSerializerJunk(MessageSerializedPropertyInfo propertyInfo)
                : base(propertyInfo)
            {
            }

            public override byte[] Serialize(int value)
            {
                _lastSerialized = value;
                return new byte[] { 0x33, 0x44, 0x55 };
            }

            public override int Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
            {
                currentArrayIndex += 3;
                return _lastSerialized;
            }
        }

        public class TestClass : IMessageSerializable
        {
            [MessageProperty(TypeSerializerClass = typeof(TypeSerializerJunk))]
            public int Int { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerUserSpecified()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, int valueToUse)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerJunk typeSerializer = new TypeSerializerJunk(propertyInfo);
            byte[] expectedArray = new byte[] { 0x33, 0x44, 0x55 };
            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        [Test]
        public void Test()
        {
            TestField(_classInfo, "Int", 3);
            TestField(_classInfo, "Int", 314352355);
            TestField(_classInfo, "Int", -3);
        }

        [Test]
        public void TestSerialization()
        {
            TestClass testClass = new TestClass();
            testClass.Int = 12345;

            byte[] serialized = Serializer.Instance.Serialize(testClass);
            Assert.That(serialized, Is.EqualTo(new byte[] { 0x33, 0x44, 0x55 }), "Serialized");

            TestClass deserialized = Serializer.Instance.Deserialize<TestClass>(serialized);
            Assert.That(deserialized.Int, Is.EqualTo(12345), "Deserialized");
        }
    }
}
