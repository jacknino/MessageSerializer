using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public interface IBlob
    {
        byte MeterSize { get; set; }
        ulong BcdValue { get; set; }
    }

    public class NoLimitationsBlob : IBlob, IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public ulong BcdValue { get; set; }
    }

    public class MinimizeVariableLengthBlob : IBlob, IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data, MinimizeVariableLength = true)]
        public ulong BcdValue { get; set; }
    }

    public class MinLengthVariableLengthBlob : IBlob, IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data, MinLength = 3, MinimizeVariableLength = true)]
        public ulong BcdValue { get; set; }
    }

    public class MaxLengthVariableLengthBlob : IBlob, IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data, MaxLength = 3, MinimizeVariableLength = true)]
        public ulong BcdValue { get; set; }
    }

    public class TestMessage : IMessageSerializable
    {
        public TestMessage()
        {
        }

        public byte MessageType { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public int NoLimitationsLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<NoLimitationsBlob> NoLimitations { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public int MinimizeVariableLengthLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<MinimizeVariableLengthBlob> MinimizeVariableLength { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public int MinLengthVariableLengthLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<MinLengthVariableLengthBlob> MinLengthVariableLength { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public int MaxLengthVariableLengthLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<MaxLengthVariableLengthBlob> MaxLengthVariableLength { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte ListWithNothingLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<NoLimitationsBlob> ListWithNothing { get; set; }
    }

    [TestFixture]
    public class TestVariableLengthBlobs : MessageUnitTestBase<TestMessage>
    {
        struct TestMeter
        {
            public TestMeter(ulong numericValue, byte[] byteArrayValue)
            {
                NumericValue = numericValue;
                ByteArrayValue = byteArrayValue;
            }

            public ulong NumericValue;
            public byte[] ByteArrayValue;
        }

        List<TestMeter> GetTestMeters()
        {
            List<TestMeter> testMeterList = new List<TestMeter>();
            testMeterList.Add(new TestMeter(0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            testMeterList.Add(new TestMeter(1, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 }));
            testMeterList.Add(new TestMeter(201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(30201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(4030201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(504030201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(60504030201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(7060504030201, new byte[] { 0x00, 0x00, 0x00, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(807060504030201, new byte[] { 0x00, 0x00, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(90807060504030201, new byte[] { 0x00, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }));
            testMeterList.Add(new TestMeter(10090807060504030201, new byte[] { 0x10, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 }));

            return testMeterList;
        }

        void AddMeter<T>(List<T> list, ulong value) where T : IBlob, new()
        {
            T meter = new T();
            meter.BcdValue = value;
            list.Add(meter);
        }

        List<T> AddMeters<T>(List<TestMeter> testMeterList) where T : IBlob, new()
        {
            List<T> list = new List<T>();
            foreach (TestMeter testMeter in testMeterList)
            {
                AddMeter(list, testMeter.NumericValue);
            }

            return list;
        }

        int CheckMeter<T>(TestMeter testMeter, byte[] bytes, ref int byteIndex, int minLength, int maxLength, bool minimizeLength) where T : IBlob, new()
        {
            string baseMessage = typeof(T).Name;
            int startByteIndex = byteIndex;

            int minimumBytesToFitFullValue = 0;
            if (minimizeLength)
            {
                for (int index = 0; index < testMeter.ByteArrayValue.Length; ++index)
                {
                    if (testMeter.ByteArrayValue[index] != 0)
                    {
                        minimumBytesToFitFullValue = testMeter.ByteArrayValue.Length - index;
                        break;
                    }
                }
            }

            int maximumMeterSize = maxLength == -1 ? testMeter.ByteArrayValue.Length : Math.Min(testMeter.ByteArrayValue.Length, maxLength);
            int minimumMeterSize = Math.Max(minLength, minimumBytesToFitFullValue);
            if (minimumMeterSize > maximumMeterSize)
                minimumMeterSize = maximumMeterSize;
            int expectedLength = (minimizeLength ? minimumMeterSize : maximumMeterSize);

            Assert.That(bytes[byteIndex++], Is.EqualTo(expectedLength), baseMessage + "MeterLength");
            byteIndex += CheckMultiByteArray(bytes, byteIndex, baseMessage + "MeterValue", GetSubArray(testMeter.ByteArrayValue, testMeter.ByteArrayValue.Length - expectedLength, expectedLength));

            return byteIndex - startByteIndex;
        }

        void CheckMeters<T>(List<TestMeter> testMeterList, byte[] bytes, ref int byteIndex, List<T> list, int minLength, int maxLength, bool minimizeLength) where T : IBlob, new()
        {
            // What we need to check
            // First get the total length of the blob
            // The length byte for the list is correct
            // The number of meters in the list is correct
            // The length byte of each value in the list is correct
            // The values in the list are correct

            string baseMessage = typeof(T).Name;
            int listLengthBytes = int.Parse(BitConverter.ToString(GetSubArray(bytes, byteIndex, 4, true)).Replace("-", ""), System.Globalization.NumberStyles.HexNumber);
            byteIndex += 4;

            int testMeterIndex = 0;
            int listBytesUsed = 0;
            while (listBytesUsed < listLengthBytes)
            {
                Assert.That(testMeterIndex, Is.LessThan(testMeterList.Count), baseMessage + "TestMeterIndex");

                listBytesUsed += CheckMeter<T>(testMeterList[testMeterIndex], bytes, ref byteIndex, minLength, maxLength, minimizeLength);
                ++testMeterIndex;
            }
            Assert.That(listBytesUsed, Is.EqualTo(listLengthBytes), baseMessage + "ListLength");
            Assert.That(testMeterIndex, Is.EqualTo(testMeterList.Count), baseMessage + "MetersTested");
        }

        void CheckDeserializedMeters<T>(List<T> originalList, List<T> deserializedList) where T : IBlob, new()
        {
            string baseMessage = typeof(T).Name;
            if (originalList == null)
            {
                Assert.That(deserializedList.Count, Is.EqualTo(0), "ListItems are empty");
                return;
            }

            Assert.That(originalList.Count, Is.EqualTo(deserializedList.Count));
            for (int index = 0; index < originalList.Count; ++index)
            {
                Assert.That(deserializedList[index].MeterSize, Is.EqualTo(originalList[index].MeterSize), baseMessage + "MeterSize");
                Assert.That(deserializedList[index].BcdValue, Is.EqualTo(deserializedList[index].BcdValue), baseMessage + "MeterValue");
            }
        }

        [Test]
        public void Test()
        {
            TestMessage testMessage = new TestMessage();
            testMessage.MessageType = 0x01;

            List<TestMeter> testMeters = GetTestMeters();
            testMessage.NoLimitations = AddMeters<NoLimitationsBlob>(testMeters);
            testMessage.MinimizeVariableLength = AddMeters<MinimizeVariableLengthBlob>(testMeters);
            testMessage.MinLengthVariableLength = AddMeters<MinLengthVariableLengthBlob>(testMeters);
            testMessage.MaxLengthVariableLength = AddMeters<MaxLengthVariableLengthBlob>(testMeters);

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                //Assert.That(bytes.Length, Is.EqualTo(17), "TotalLength");
                int byteIndex = 0;
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MessageType), "MessageType");
                CheckMeters(testMeters, bytes, ref byteIndex, testMessage.NoLimitations, 0, -1, false);
                CheckMeters(testMeters, bytes, ref byteIndex, testMessage.MinimizeVariableLength, 0, -1, true);
                CheckMeters(testMeters, bytes, ref byteIndex, testMessage.MinLengthVariableLength, 3, -1, true);
                CheckMeters(testMeters, bytes, ref byteIndex, testMessage.MaxLengthVariableLength, 0, 3, true);
                Assert.That(serialized.ListWithNothingLength, Is.EqualTo(0), "ListWithNothingLength");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.ListWithNothingLength), "ListWithNothingLength");
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MessageType, Is.EqualTo(originalObject.MessageType), "MessageType");
                CheckDeserializedMeters(originalObject.NoLimitations, deserializedObject.NoLimitations);
                CheckDeserializedMeters(originalObject.MinimizeVariableLength, deserializedObject.MinimizeVariableLength);
                CheckDeserializedMeters(originalObject.MinLengthVariableLength, deserializedObject.MinLengthVariableLength);
                CheckDeserializedMeters(originalObject.MaxLengthVariableLength, deserializedObject.MaxLengthVariableLength);
                CheckDeserializedMeters(originalObject.ListWithNothing, deserializedObject.ListWithNothing);
            });
        }
    }

    public class GapBlob : IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }

        public int SomeOtherField { get; set; }
        public short OtherField { get; set; }

        [MessageProperty(BlobType = BlobTypes.Data)]
        public ulong BcdValue { get; set; }
    }

    [TestFixture]
    public class TestGapBlob : MessageUnitTestBase<GapBlob>
    {
        [Test]
        public void Test()
        {
            GapBlob testMessage = new GapBlob();
            testMessage.SomeOtherField = 12345;
            testMessage.OtherField = 23;
            testMessage.BcdValue = 99887766;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                //Assert.That(bytes.Length, Is.EqualTo(17), "TotalLength");
                int byteIndex = 0;
                Assert.That(serialized.MeterSize, Is.EqualTo(10), "MeterSizeProperty");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MeterSize), "MeterSize");
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherField", serialized.SomeOtherField);
                byteIndex += CheckNumeric(bytes, byteIndex, "OtherField", serialized.OtherField);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdValue", serialized.BcdValue);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MeterSize, Is.EqualTo(originalObject.MeterSize), "MeterSize");
                Assert.That(deserializedObject.SomeOtherField, Is.EqualTo(originalObject.SomeOtherField), "SomeOtherField");
                Assert.That(deserializedObject.OtherField, Is.EqualTo(originalObject.OtherField), "OtherField");
                Assert.That(deserializedObject.BcdValue, Is.EqualTo(originalObject.BcdValue), "BcdValue");
            });
        }
    }

    public class DoubleGapBlob : IMessageSerializable
    {
        [MessageProperty(BlobType = BlobTypes.Length, AssociatedBlobProperty = "BcdValue2")]
        public byte MeterSize2 { get; set; }

        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte MeterSize { get; set; }

        public int SomeOtherField { get; set; }
        public short OtherField { get; set; }

        [MessageProperty(BlobType = BlobTypes.Data)]
        public ulong BcdValue { get; set; }

        [MessageProperty(BlobType = BlobTypes.Data)]
        public int BcdValue2 { get; set; }
    }

    [TestFixture]
    public class TestDoubleGapBlob : MessageUnitTestBase<DoubleGapBlob>
    {
        [Test]
        public void Test()
        {
            DoubleGapBlob testMessage = new DoubleGapBlob();
            testMessage.SomeOtherField = 12345;
            testMessage.OtherField = 23;
            testMessage.BcdValue = 99887766;
            testMessage.BcdValue2 = 5544;

            byte[] serializedBytes = TestSerialize(testMessage, (bytes, serialized) =>
            {
                //Assert.That(bytes.Length, Is.EqualTo(17), "TotalLength");
                int byteIndex = 0;
                Assert.That(serialized.MeterSize2, Is.EqualTo(5), "MeterSize2Property");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MeterSize2), "MeterSize2");
                Assert.That(serialized.MeterSize, Is.EqualTo(10), "MeterSizeProperty");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.MeterSize), "MeterSize");
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeOtherField", serialized.SomeOtherField);
                byteIndex += CheckNumeric(bytes, byteIndex, "OtherField", serialized.OtherField);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdValue", serialized.BcdValue);
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdValue2", serialized.BcdValue2);
            });

            TestDeserialize(serializedBytes, testMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.MeterSize, Is.EqualTo(originalObject.MeterSize), "MeterSize");
                Assert.That(deserializedObject.MeterSize2, Is.EqualTo(originalObject.MeterSize2), "MeterSize2");
                Assert.That(deserializedObject.SomeOtherField, Is.EqualTo(originalObject.SomeOtherField), "SomeOtherField");
                Assert.That(deserializedObject.OtherField, Is.EqualTo(originalObject.OtherField), "OtherField");
                Assert.That(deserializedObject.BcdValue, Is.EqualTo(originalObject.BcdValue), "BcdValue");
                Assert.That(deserializedObject.BcdValue2, Is.EqualTo(originalObject.BcdValue2), "BcdValue2");
            });
        }
    }

}
