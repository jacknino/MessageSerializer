using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestTypeSerializerDateTime : TestTypeSerializerBase
    {
        public class TestClass
        {
            public DateTime Default { get; set; } // Format: MMddyyyyHHmmss - 7 bytes

            [MessageProperty(Format = "yyyyMMddHHmmssyyyy")] // 9 bytes
            public DateTime Format { get; set; }

            [MessageProperty(Format = "yyyyMMddHHmmssffff")] // 9 bytes
            public DateTime FormatIncludeMilliseconds { get; set; }

            //[MessageSerializedProperty(Format = "yyyyMMddHHmmssyyyy", Length = 5)] // 9 bytes, but only send 5
            //public DateTime FormatLengthShort { get; set; }

            //[MessageSerializedProperty(Format = "yyyyMMddHHmmssyyyy", Length = 12)] // 9 bytes, but send 12
            //public DateTime FormatLengthLong { get; set; }

            //[MessageSerializedProperty(IsBcd = false)]
            //public DateTime NonBcd { get; set; }
        }

        protected MessageSerializedClassInfo _classInfo;

        public TestTypeSerializerDateTime()
        {
            _classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass));
        }

        protected int GetOutputLength(string format)
        {
            return (format.Length + 1) / 2;
        }

        protected void TestField(MessageSerializedClassInfo classInfo, string propertyName, DateTime valueToUse)
        {
            MessageSerializedPropertyInfo propertyInfo = GetPropertyInfo(classInfo, propertyName);
            TypeSerializerDateTime typeSerializer = new TypeSerializerDateTime(propertyInfo);

            string format = propertyInfo.MessagePropertyAttribute.Format;
            string formattedValue = valueToUse.ToString(format);

            // TODO: This uses the same method as the class which is not really what we want
            byte[] expectedArray = ArrayOps.GetBcdBytes(Convert.ToUInt64(formattedValue), GetOutputLength(format));

            TestField(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected override void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            // The "Kind" property after deserialization is "Unspecified" so we need to ignore that part
            DateTime expectedDateTime = (DateTime)(object)expectedValue;
            DateTime deserializedDateTime = DateTime.SpecifyKind((DateTime)(object)deserializedValue, expectedDateTime.Kind);
            base.VerifyDeserialized(deserializedDateTime, expectedDateTime, propertyInfo, valueArray);
        }

        protected override string GetExpectedToStringValueFormat(MessageSerializedPropertyInfo propertyInfo)
        {
            return $"{{0:yyyy-MM-dd HH:mm:ss}}";
        }

        protected void TestFieldWithValues(MessageSerializedClassInfo classInfo, string propertyName, DateTime[] valuesToUse)
        {
            foreach (DateTime value in valuesToUse)
            {
                TestField(classInfo, propertyName, value);
            }
        }

        protected DateTime GetModifiedNow(bool includeMilliseconds)
        {
            // DateTime.Now can have a ticks value that includes nanosecond parts that aren't easily modifiable.
            // We also might not want the milliseconds because at least for some of our tests they aren't included.
            DateTime now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, includeMilliseconds ? now.Millisecond : 0);
        }

        [Test]
        public void TestDefault()
        {
            TestFieldWithValues(_classInfo, "Default", new DateTime[] { GetModifiedNow(false), new DateTime(2000, 1, 1, 12, 22, 15) });
        }

        [Test]
        public void TestFormat()
        {
            // Because we aren't using milliseconds in the format we need to make sure they are 0, otherwise the deserialize won't match
            TestFieldWithValues(_classInfo, "Format", new DateTime[] { GetModifiedNow(false), new DateTime(2000, 1, 1, 12, 22, 15) });
        }

        [Test]
        public void TestFormatIncludeMilliseconds()
        {
            // Because we aren't using milliseconds in the format we need to make sure they are 0, otherwise the deserialize won't match
            TestFieldWithValues(_classInfo, "FormatIncludeMilliseconds", new DateTime[] { GetModifiedNow(false), new DateTime(2000, 1, 1, 12, 22, 15) });
        }
    }
}
