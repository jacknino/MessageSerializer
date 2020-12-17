using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestToString
    {
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

        protected void VerifyToString<TTestClassType>(TTestClassType testObject, string expectedToStringResult, ToStringFormatProperties formatProperties)
            where TTestClassType : IMessageSerializable
        {
            string actualToStringResult = Serializer.Instance.ToString(testObject, false, 0, null, null, false, formatProperties);
            Assert.That(actualToStringResult, Is.EqualTo(expectedToStringResult), "ToString");
        }

        protected TestClass GetTestObject()
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

            // To set the length fields we serialize the object.  We don't actually need the result
            Serializer.Instance.Serialize(testClass);

            return testClass;
        }

        [Test]
        public void TestDefault()
        {
            string expectedToStringResult = "SubClass: \r\n" +
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

            ToStringFormatProperties formatProperties = ToStringFormatProperties.Default;
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestNoIndentFields()
        {
            string expectedToStringResult = "SubClass: \r\n" +
                "Length: 29 (0x1D), \r\n" +
                "Meters: \r\n" +
                "    Index 0: \r\n" +
                "        MeterNumber: 8738 (0x2222), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 123456\r\n" +
                "    Index 1: \r\n" +
                "        MeterNumber: 1 (0x0001), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 2\r\n" +
                "    Index 2: \r\n" +
                "        MeterNumber: 9029 (0x2345), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 1666666666, \r\n" +
                "SomeSubClass: \r\n" +
                "Field1: 3 (0x03), \r\n" +
                "Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.Fields.Indent = false;

            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestNoIndentListItemHeaders()
        {
            string expectedToStringResult = "SubClass: \r\n" +
                "    Length: 29 (0x1D), \r\n" +
                "    Meters: \r\n" +
                "    Index 0: \r\n" +
                "        MeterNumber: 8738 (0x2222), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 123456\r\n" +
                "    Index 1: \r\n" +
                "        MeterNumber: 1 (0x0001), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 2\r\n" +
                "    Index 2: \r\n" +
                "        MeterNumber: 9029 (0x2345), \r\n" +
                "        MeterLength: 5 (0x05), \r\n" +
                "        BcdMeterValue: 1666666666, \r\n" +
                "    SomeSubClass: \r\n" +
                "        Field1: 3 (0x03), \r\n" +
                "        Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.ListItemHeaders.Indent = false;
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestNoIndentListItems()
        {
            // Note: The list items are still indented since Fields are being indented
            string expectedToStringResult = "SubClass: \r\n" +
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

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.ListItems.Indent = false;
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestNoIndentListItemsOrFields()
        {
            string expectedToStringResult = "SubClass: \r\n" +
                "Length: 29 (0x1D), \r\n" +
                "Meters: \r\n" +
                "    Index 0: \r\n" +
                "    MeterNumber: 8738 (0x2222), \r\n" +
                "    MeterLength: 5 (0x05), \r\n" +
                "    BcdMeterValue: 123456\r\n" +
                "    Index 1: \r\n" +
                "    MeterNumber: 1 (0x0001), \r\n" +
                "    MeterLength: 5 (0x05), \r\n" +
                "    BcdMeterValue: 2\r\n" +
                "    Index 2: \r\n" +
                "    MeterNumber: 9029 (0x2345), \r\n" +
                "    MeterLength: 5 (0x05), \r\n" +
                "    BcdMeterValue: 1666666666, \r\n" +
                "SomeSubClass: \r\n" +
                "Field1: 3 (0x03), \r\n" +
                "Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.Fields.Indent = false;
            formatProperties.ListItems.Indent = false;
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestFieldsNoSeparateLine()
        {
            string expectedToStringResult = "SubClass: " +
                "Length: 29 (0x1D), " +
                "Meters: \r\n" +
                "    Index 0: \r\n" +
                "        MeterNumber: 8738 (0x2222), " +
                "MeterLength: 5 (0x05), " +
                "BcdMeterValue: 123456\r\n" +
                "    Index 1: \r\n" +
                "        MeterNumber: 1 (0x0001), " +
                "MeterLength: 5 (0x05), " +
                "BcdMeterValue: 2\r\n" +
                "    Index 2: \r\n" +
                "        MeterNumber: 9029 (0x2345), " +
                "MeterLength: 5 (0x05), " +
                "BcdMeterValue: 1666666666, " +
                "SomeSubClass: " +
                "Field1: 3 (0x03), " +
                "Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.Fields.SeparateLine = false;
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestFieldPrefixSuffix()
        {
            string expectedToStringResult = "<Field>SubClass: \r\n" +
                "    <Field>Length: 29 (0x1D)</Field>, \r\n" +
                "    <Field>Meters: \r\n" +
                "        Index 0: \r\n" +
                "            <Field>MeterNumber: 8738 (0x2222)</Field>, \r\n" +
                "            <Field>MeterLength: 5 (0x05)</Field>, \r\n" +
                "            <Field>BcdMeterValue: 123456</Field>\r\n" +
                "        Index 1: \r\n" +
                "            <Field>MeterNumber: 1 (0x0001)</Field>, \r\n" +
                "            <Field>MeterLength: 5 (0x05)</Field>, \r\n" +
                "            <Field>BcdMeterValue: 2</Field>\r\n" +
                "        Index 2: \r\n" +
                "            <Field>MeterNumber: 9029 (0x2345)</Field>, \r\n" +
                "            <Field>MeterLength: 5 (0x05)</Field>, \r\n" +
                "            <Field>BcdMeterValue: 1666666666</Field></Field>, \r\n" +
                "    <Field>SomeSubClass: \r\n" +
                "        <Field>Field1: 3 (0x03)</Field>, \r\n" +
                "        <Field>Field2: -12345666 (0xFF439EBE)</Field></Field></Field>";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.Fields.Prefix = "<Field>";
            formatProperties.Fields.Suffix = "</Field>";
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void TestListHeadersPrefixSuffix()
        {
            string expectedToStringResult = "SubClass: \r\n" +
                "    Length: 29 (0x1D), \r\n" +
                "    Meters: \r\n" +
                "        <LH>Index 0: \r\n" +
                "            MeterNumber: 8738 (0x2222), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 123456</LH>\r\n" +
                "        <LH>Index 1: \r\n" +
                "            MeterNumber: 1 (0x0001), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 2</LH>\r\n" +
                "        <LH>Index 2: \r\n" +
                "            MeterNumber: 9029 (0x2345), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 1666666666</LH>, \r\n" +
                "    SomeSubClass: \r\n" +
                "        Field1: 3 (0x03), \r\n" +
                "        Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.ListItemHeaders.Prefix = "<LH>";
            formatProperties.ListItemHeaders.Suffix = "</LH>";
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }

        [Test]
        public void ListItemsPrefixSuffix()
        {
            string expectedToStringResult = "SubClass: \r\n" +
                "    Length: 29 (0x1D), \r\n" +
                "    Meters: \r\n" +
                "        Index 0: \r\n" +
                "            <LI>MeterNumber: 8738 (0x2222), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 123456</LI>\r\n" +
                "        Index 1: \r\n" +
                "            <LI>MeterNumber: 1 (0x0001), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 2</LI>\r\n" +
                "        Index 2: \r\n" +
                "            <LI>MeterNumber: 9029 (0x2345), \r\n" +
                "            MeterLength: 5 (0x05), \r\n" +
                "            BcdMeterValue: 1666666666</LI>, \r\n" +
                "    SomeSubClass: \r\n" +
                "        Field1: 3 (0x03), \r\n" +
                "        Field2: -12345666 (0xFF439EBE)";

            ToStringFormatProperties formatProperties = new ToStringFormatProperties();
            formatProperties.ListItems.Prefix = "<LI>";
            formatProperties.ListItems.Suffix = "</LI>";
            VerifyToString(GetTestObject(), expectedToStringResult, formatProperties);
        }
    }
}
