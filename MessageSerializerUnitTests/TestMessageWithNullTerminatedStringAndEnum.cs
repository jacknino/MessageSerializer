using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public enum Enum1
    {
        Value1 = 0x01,
        Value2 = 0x02
    }

    public enum Enum2 : byte
    {
        Something1 = 1,
        Something2
    }

    public class CopyToThumbDriveCopyingFileMessage : IMessageSerializable
    {
        public Enum1 Enum1Value { get; set; }
        public Enum2 Enum2Value { get; set; }

        // These fields are expected to be null terminated strings but always take up 260 bytes in the message
        [MessageProperty(Length = 30)]
        public string SourceFile { get; set; }
        [MessageProperty(Length = 30)]
        public string OneShort { get; set; }
        [MessageProperty(Length = 30)]
        public string ExactLength { get; set; }
        [MessageProperty(Length = 30)]
        public string OneLong { get; set; }
        [MessageProperty(Length = 30)]
        public string ZeroLength { get; set; }
        public int FileSize { get; set; }
        //public FileStats FileStats { get; set; }
    }

    [TestFixture]
    public class TestMessageWithNullTerminatedStringAndEnum : MessageUnitTestBase<CopyToThumbDriveCopyingFileMessage>
    {
        [Test]
        public void Test()
        {
            CopyToThumbDriveCopyingFileMessage copyToThumbDriveCopyingFileMessage = new CopyToThumbDriveCopyingFileMessage();
            copyToThumbDriveCopyingFileMessage.Enum1Value = Enum1.Value1;
            copyToThumbDriveCopyingFileMessage.Enum2Value = Enum2.Something2;
            copyToThumbDriveCopyingFileMessage.SourceFile = "/var/local/blah/file.txt";
            copyToThumbDriveCopyingFileMessage.OneShort = "12345678901234567890123456789";
            copyToThumbDriveCopyingFileMessage.ExactLength = "123456789012345678901234567890";
            copyToThumbDriveCopyingFileMessage.OneLong = "1234567890123456789012345678901";
            copyToThumbDriveCopyingFileMessage.ZeroLength = "";
            copyToThumbDriveCopyingFileMessage.FileSize = 0x1234;

            byte[] serializedBytes = TestSerialize(copyToThumbDriveCopyingFileMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                // Length = 4 for Enum1, 1 for Enum2, 30 + 30 + 30 + 30 + 30 for source, OneShort, ExactLength, OneLong and ZeroLength, 4 for FileSize = 159
                // Should look into sizeof class to see if that is correct
                Assert.That(bytes.Length, Is.EqualTo(159), "Length");
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Enum1", new byte[] {0x01, 0x00, 0x00, 0x00});
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "Enum2", new byte[] { 0x02 });
                byteIndex += CheckStringMatches(bytes, byteIndex, "SourceFile", copyToThumbDriveCopyingFileMessage.SourceFile, 30);
                byteIndex += CheckStringMatches(bytes, byteIndex, "OneShort", copyToThumbDriveCopyingFileMessage.OneShort, 30);
                byteIndex += CheckStringMatches(bytes, byteIndex, "ExactLength", copyToThumbDriveCopyingFileMessage.ExactLength, 30);
                byteIndex += CheckStringMatches(bytes, byteIndex, "OneLong", copyToThumbDriveCopyingFileMessage.OneLong.Substring(0, copyToThumbDriveCopyingFileMessage.OneLong.Length - 1), 30);
                byteIndex += CheckStringMatches(bytes, byteIndex, "ZeroLength", copyToThumbDriveCopyingFileMessage.ZeroLength, 30);
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "FileSize", new byte[] { 0x34, 0x12, 0x00, 0x00 });
            });

            TestDeserialize(serializedBytes, copyToThumbDriveCopyingFileMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Enum1Value, Is.EqualTo(originalObject.Enum1Value), "Enum1Value");
                Assert.That(deserializedObject.Enum2Value, Is.EqualTo(originalObject.Enum2Value), "Enum2Value");
                Assert.That(deserializedObject.SourceFile, Is.EqualTo(originalObject.SourceFile), "SourceFile");
                Assert.That(deserializedObject.OneShort, Is.EqualTo(originalObject.OneShort), "OneShort");
                Assert.That(deserializedObject.ExactLength, Is.EqualTo(originalObject.ExactLength), "ExactLength");
                Assert.That(deserializedObject.OneLong, Is.EqualTo(originalObject.OneLong.Substring(0, originalObject.OneLong.Length - 1)), "OneLong");
                Assert.That(deserializedObject.ZeroLength, Is.EqualTo(originalObject.ZeroLength), "ZeroLength");
                Assert.That(deserializedObject.FileSize, Is.EqualTo(originalObject.FileSize), "FileSize");
            });
        }
    }
}
