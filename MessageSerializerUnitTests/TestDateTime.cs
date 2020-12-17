using System;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class TestDateTimeMessage : IMessageSerializable
    {
        public DateTime Default { get; set; } // Format: MMddyyyyHHmmss - 7 bytes

        [MessageProperty(Format = "yyyyMMddHHmmssyyyy")] // 9 bytes
        public DateTime Format { get; set; }

        //[MessageSerializedProperty(Format = "yyyyMMddHHmmssyyyy", Length = 5)] // 9 bytes, but only send 5
        //public DateTime FormatLengthShort { get; set; }

        //[MessageSerializedProperty(Format = "yyyyMMddHHmmssyyyy", Length = 12)] // 9 bytes, but send 12
        //public DateTime FormatLengthLong { get; set; }

        //[MessageSerializedProperty(IsBcd = false)]
        //public DateTime NonBcd { get; set; }
    }

    [TestFixture]
    public class TestDateTime : MessageUnitTestBase<TestDateTimeMessage>
    {
        [Test]
        public void Test()
        {
            TestDateTimeMessage testMessage = new TestDateTimeMessage();
            testMessage.Default = new DateTime(2020, 1, 2, 12, 34, 56);
            testMessage.Format = new DateTime(1970, 12, 11, 3, 4, 5);
            //testMessage.FormatLengthShort = testMessage.Format;
            //testMessage.FormatLengthLong = testMessage.Format;
            //testMessage.NonBcd = new DateTime(2019, 2, 1, 21, 43, 55);

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                //Assert.That(bytes.Length, Is.EqualTo(28));
                Assert.That(bytes.Length, Is.EqualTo(16));
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Default", new byte[] { 0x01, 0x02, 0x20, 0x20, 0x12, 0x34, 0x56 });
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Format", new byte[] { 0x19, 0x70, 0x12, 0x11, 0x03, 0x04, 0x05, 0x19, 0x70 });
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "FormatLengthShort", new byte[] { 0x19, 0x70, 0x12, 0x11, 0x03 });
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "FormatLengthLong", new byte[] { 0x19, 0x70, 0x12, 0x11, 0x03, 0x04, 0x05, 0x19, 0x70, 0x00, 0x00, 0x00 });
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "NonBcd", new byte[] { 0x01, 0x02, 0x20, 0x20, 0x12, 0x34, 0x56 });
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Default, Is.EqualTo(originalObject.Default), "Default");
                Assert.That(deserializedObject.Format, Is.EqualTo(originalObject.Format), "Format");
                //Assert.That(deserializedObject.FormatLengthShort, Is.EqualTo(originalObject.FormatLengthShort), "FormatLengthShort");
                //Assert.That(deserializedObject.FormatLengthLong, Is.EqualTo(originalObject.FormatLengthLong), "FormatLengthLong");
                //Assert.That(deserializedObject.NonBcd, Is.EqualTo(originalObject.NonBcd), "NonBcd");
            });
        }
    }
}
