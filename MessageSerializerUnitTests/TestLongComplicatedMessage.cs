using System;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class LongComplicatedMessage : IMessageSerializable
    {
        public LongComplicatedMessage()
        {
            SomeList = new List<uint>();
            SomeSubClass = new SubClass();
            Meters = new List<MetersBlob>();
        }

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

        public byte Address { get; set; }
        public byte Command { get; set; }
        //[MessageProperty(MessageLengthType = MessageLengthTypes.RestOfMessage)]
        public byte Length { get; set; }
        public uint BcdSomething { get; set; }
        public DateTime BcdDateTime { get; set; }
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte SomeListLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public List<uint> SomeList { get; set; } 
        //public List<uint> VariableLengthList { get; set; } 
        [MessageProperty(BlobType = BlobTypes.Length)]
        public byte SomeStringLength { get; set; }
        [MessageProperty(BlobType = BlobTypes.Data)]
        public string SomeStringData { get; set; }
        [MessageProperty(Length = 3)]
        public string String3Fixed { get; set; }
        [MessageProperty(Length = 6)]
        public string String6Fixed { get; set; }
        public List<MetersBlob> Meters { get; set; }
        //[MessageSerializedProperty(Length = 6, VariableLength = true)]
        //public string String6Var { get; set; }
        public SubClass SomeSubClass { get; set; }
        public ushort Crc { get; set; }
    }

    [TestFixture]
    public class TestLongComplicatedMessage : MessageUnitTestBase<LongComplicatedMessage>
    {
        [Test]
        public void Test()
        {
            //LongComplicatedMessage.MetersBlob meterBlob = new LongComplicatedMessage.MetersBlob();
            //meterBlob.MeterNumber = 23;
            //meterBlob.BcdMeterValue = 54321;
            //Serializer.Instance.Serialize(meterBlob);

            DateTime dateTimeMessage = new DateTime(2019, 01, 31, 16, 42, 33);
            LongComplicatedMessage longComplicatedMessage = new LongComplicatedMessage();
            longComplicatedMessage.Address = 0x01;
            longComplicatedMessage.Command = 0x88;
            //longComplicatedMessage.Length = ??;
            longComplicatedMessage.BcdSomething = 12345;
            longComplicatedMessage.BcdDateTime = dateTimeMessage;
            //longComplicatedMessage.SomeListLength = ??;
            longComplicatedMessage.SomeList.Add(3);
            longComplicatedMessage.SomeList.Add(4);
            //longComplicatedMessage.SomeStringLength = ??;
            longComplicatedMessage.SomeStringData = "9876543210";
            longComplicatedMessage.String3Fixed = "123";
            longComplicatedMessage.String6Fixed = "ABC";
            //longComplicatedMessage.Meters;
            LongComplicatedMessage.MetersBlob meter1 = new LongComplicatedMessage.MetersBlob();
            meter1.MeterNumber = 0x0056;
            //meter1.MeterLength = ??;
            meter1.BcdMeterValue = 7890;
            longComplicatedMessage.Meters.Add(meter1);

            LongComplicatedMessage.MetersBlob meter2 = new LongComplicatedMessage.MetersBlob();
            meter2.MeterNumber = 0x0078;
            //meter2.MeterLength = ??;
            meter2.BcdMeterValue = 1234;
            longComplicatedMessage.Meters.Add(meter2);

            //longComplicatedMessage.String6Var = "ZXY";
            longComplicatedMessage.SomeSubClass.Field1 = 9;
            longComplicatedMessage.SomeSubClass.Field2 = 0x66778899;
            //longComplicatedMessage.Crc = ??;

            byte[] serializedBytes = TestSerialize(longComplicatedMessage, (bytes, serialized) =>
            {
                int byteIndex = 0;
                Assert.That(bytes.Length, Is.EqualTo(67), "Length");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Address), "Address");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Command), "MessageType");
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Length), "Length");
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "BcdSomething", new byte[] { 0x00, 0x01, 0x23, 0x45 });
                byteIndex += CheckNumericBcd(bytes, byteIndex, "BcdSomething", serialized.BcdSomething);

                //_format = "MMddyyyyHHmmss";
                byteIndex += CheckMultiByteArray(bytes, byteIndex, "BcdDateTime", new byte[] { 0x01, 0x31, 0x20, 0x19, 0x16, 0x42, 0x33 });
                // Until we sort out checking the time
                //byteIndex += 7;
                Assert.That(bytes[byteIndex++], Is.EqualTo(8), "SomeListLength");
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "SomeList0", new byte[] { 0x03, 0x00, 0x00, 0x00 });
                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "SomeList1", new byte[] { 0x04, 0x00, 0x00, 0x00 });
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeList0", serialized.SomeList[0]);
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeList1", serialized.SomeList[1]);

                Assert.That(bytes[byteIndex++], Is.EqualTo(10), "SomeStringLength");
                CheckMultiByteArray(bytes, byteIndex, "SomeStringData", new byte[] { 0x39, 0x38, 0x37, 0x36, 0x35, 0x34, 0x33, 0x32, 0x31, 0x30 });
                byteIndex += CheckStringMatches(bytes, byteIndex, "SomeStringData", serialized.SomeStringData, serialized.SomeStringLength);
                CheckMultiByteArray(bytes, byteIndex, "String3Fixed", new byte[] { 0x31, 0x32, 0x33 });
                byteIndex += CheckStringMatches(bytes, byteIndex, "String3Fixed", serialized.String3Fixed, 3);
                CheckMultiByteArray(bytes, byteIndex, "String6Fixed", new byte[] { 0x41, 0x42, 0x43, 0x00, 0x00, 0x00 });
                byteIndex += CheckStringMatches(bytes, byteIndex, "String6Fixed", serialized.String6Fixed, 6);

                // Meter1
                byteIndex += CheckNumeric(bytes, byteIndex, "MeterNumber1", serialized.Meters[0].MeterNumber);
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Meters[0].MeterLength), "MeterLength1");
                CheckMultiByteArray(bytes, byteIndex, "MeterValue1", new byte[] { 0x00, 0x00, 0x00, 0x78, 0x90 });
                byteIndex += CheckNumericBcd(bytes, byteIndex, serialized.Meters[0].MeterLength, "MeterValue1", serialized.Meters[0].BcdMeterValue);

                // Meter 2
                byteIndex += CheckNumeric(bytes, byteIndex, "MeterNumber2", serialized.Meters[1].MeterNumber);
                Assert.That(bytes[byteIndex++], Is.EqualTo(serialized.Meters[1].MeterLength), "MeterLength2");
                CheckMultiByteArray(bytes, byteIndex, "MeterValue2", new byte[] { 0x00, 0x00, 0x00, 0x12, 0x34 });
                byteIndex += CheckNumericBcd(bytes, byteIndex, serialized.Meters[1].MeterLength, "MeterValue2", serialized.Meters[1].BcdMeterValue);

                //byteIndex += CheckMultiByteArray(bytes, byteIndex, "String6Var", new byte[] { 0x5A, 0x58, 0x59 });

                Assert.That(bytes[byteIndex++], Is.EqualTo(9), "SomeSubClass.Field1");
                CheckMultiByteArray(bytes, byteIndex, "SomeSubClass.Field2", new byte[] { 0x99, 0x88, 0x77, 0x66 });
                byteIndex += CheckNumeric(bytes, byteIndex, "SomeSubClass.Field2", serialized.SomeSubClass.Field2);

                Assert.That(serialized.Crc, Is.EqualTo(0x17BF), "Crc Field Value");
                byteIndex += CheckNumeric<ushort>(bytes, byteIndex, "Crc", serialized.Crc);
            });

            TestDeserialize(serializedBytes, longComplicatedMessage, (deserializedObject, bytes, originalObject) =>
            {
                Assert.That(deserializedObject.Address, Is.EqualTo(originalObject.Address), "Address");
                Assert.That(deserializedObject.Command, Is.EqualTo(originalObject.Command), "Command");
                Assert.That(deserializedObject.Length, Is.EqualTo(originalObject.Length), "Length");
                Assert.That(deserializedObject.BcdSomething, Is.EqualTo(originalObject.BcdSomething), "BcdSomething");
                Assert.That(deserializedObject.BcdDateTime, Is.EqualTo(originalObject.BcdDateTime), "BcdDateTime");
                Assert.That(deserializedObject.SomeListLength, Is.EqualTo(originalObject.SomeListLength), "SomeListLength");
                Assert.That(deserializedObject.SomeList.Count, Is.EqualTo(originalObject.SomeList.Count), "SomeList.Count");
                for (int index = 0; index < deserializedObject.SomeList.Count; ++index)
                    Assert.That(deserializedObject.SomeList[index], Is.EqualTo(originalObject.SomeList[index]), string.Format("SomeList Item {0}", index));

                Assert.That(deserializedObject.SomeStringLength, Is.EqualTo(originalObject.SomeStringLength), "SomeStringLength");
                Assert.That(deserializedObject.SomeStringData, Is.EqualTo(originalObject.SomeStringData), "SomeStringData");
                Assert.That(deserializedObject.String3Fixed, Is.EqualTo(originalObject.String3Fixed), "String3Fixed");
                Assert.That(deserializedObject.String6Fixed, Is.EqualTo(originalObject.String6Fixed), "String6Fixed");
                Assert.That(deserializedObject.Meters.Count, Is.EqualTo(originalObject.Meters.Count), "Meters.Count");
                for (int index = 0; index < deserializedObject.Meters.Count; ++index)
                {
                    Assert.That(deserializedObject.Meters[index].MeterNumber, Is.EqualTo(originalObject.Meters[index].MeterNumber), string.Format("MetersItem{0}.MeterNumber", index));
                    Assert.That(deserializedObject.Meters[index].MeterLength, Is.EqualTo(originalObject.Meters[index].MeterLength), string.Format("MetersItem{0}.MeterLength", index));
                    Assert.That(deserializedObject.Meters[index].BcdMeterValue, Is.EqualTo(originalObject.Meters[index].BcdMeterValue), string.Format("MetersItem{0}.BcdMeterValue", index));
                }

                Assert.That(deserializedObject.SomeSubClass.Field1, Is.EqualTo(originalObject.SomeSubClass.Field1), "SomeSubClass.Field1");
                Assert.That(deserializedObject.SomeSubClass.Field2, Is.EqualTo(originalObject.SomeSubClass.Field2), "SomeSubClass.Field2");
                Assert.That(deserializedObject.Crc, Is.EqualTo(originalObject.Crc), "Crc");
            });
        }

        [Test]
        public void TestClassInfoString()
        {
            // There's no actual test here other than to check what the GetClassInfoString function returns
            string classInfoString = Serializer.Instance.GetClassInfoString(typeof(LongComplicatedMessage));
            Console.WriteLine(classInfoString);
        }
    }
}
