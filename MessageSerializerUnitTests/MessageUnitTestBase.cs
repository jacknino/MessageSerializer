using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public class MessageUnitTestBase<T> where T : class, IMessageSerializable
    {
        protected delegate void VerifySerializedBytesDelegate(byte[] serializedBytes, T objectSerialized);
        protected delegate void VerifyDeserializedObjectDelegate(T deserializedObject, byte[] originalBytes, T originalObject);

        protected MessageUnitTestBase()
        {
        }

        protected byte[] TestSerialize(T objectToSerialize, VerifySerializedBytesDelegate verifyFunction)
        {
            // We want to test loading by attributes and by file
            Serializer.Instance.GetClassInfo(typeof(T), true);
            byte[] serializedBytes = Serializer.Instance.Serialize(objectToSerialize);
            verifyFunction(serializedBytes, objectToSerialize);

            ConfigMessageSerializerClass.WriteDefaultToFile(typeof(T));
            ConfigMessageSerializerClass configMessageSerializerClass = ConfigMessageSerializerClass.ReadFromFile(typeof(T));
            List<ConfigMessageSerializerClass> classList = new List<ConfigMessageSerializerClass>() { configMessageSerializerClass };
            Serializer.Instance.GetClassInfo(typeof(T), classList, true);
            byte[] serializedBytesConfig = Serializer.Instance.Serialize(objectToSerialize);
            verifyFunction(serializedBytesConfig, objectToSerialize);

            Assert.That(serializedBytes.SequenceEqual(serializedBytesConfig), Is.True, "BytesEqual");
            return serializedBytes;
        }

        protected T TestDeserialize(byte[] bytesToDeserialize, T originalObject, VerifyDeserializedObjectDelegate verifyFunction)
        {
            Serializer.Instance.GetClassInfo(typeof(T), true);
            T deserializedObject = Serializer.Instance.Deserialize<T>(bytesToDeserialize);
            verifyFunction(deserializedObject, bytesToDeserialize, originalObject);

            ConfigMessageSerializerClass.WriteDefaultToFile(typeof(T));
            ConfigMessageSerializerClass configMessageSerializerClass = ConfigMessageSerializerClass.ReadFromFile(typeof(T));
            List<ConfigMessageSerializerClass> classList = new List<ConfigMessageSerializerClass>() { configMessageSerializerClass };
            Serializer.Instance.GetClassInfo(typeof(T), classList, true);
            T deserializedObjectConfig = Serializer.Instance.Deserialize<T>(bytesToDeserialize);
            verifyFunction(deserializedObjectConfig, bytesToDeserialize, originalObject);

            return deserializedObject;
        }

        protected string GetHexStringFromArrayNumeric<NumericType>(byte[] fullArray, int startIndex) where NumericType : struct
        {
            return GetHexStringFromArray(fullArray, startIndex, System.Runtime.InteropServices.Marshal.SizeOf(typeof(NumericType)), true);
        }

        //protected int CheckNumeric<NumericType>(byte[] fullArray, int startIndex, string itemName, NumericType expectedValue, Func<string, NumberStyles, NumericType> parseFunction) where NumericType : struct
        //{
        //    int length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NumericType));
        //    string hexString = GetHexStringFromArray(fullArray, startIndex, length, true);
        //    NumericType receivedValue = parseFunction(hexString, NumberStyles.HexNumber);
        //    Assert.That(receivedValue, Is.EqualTo(expectedValue), itemName);

        //    return length;
        //}

        protected int CheckNumeric<TNumericType>(TNumericType receivedValue, string itemName, TNumericType expectedValue) where TNumericType : struct
        {
            Assert.That(receivedValue, Is.EqualTo(expectedValue), itemName);
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(TNumericType));
        }

        protected int CheckNumeric<TNumericType>(byte[] fullArray, int startIndex, string itemName, TNumericType expectedValue) where TNumericType : struct
        {
            TNumericType receivedValue = ArrayOps.GetNumeric<TNumericType>(fullArray, startIndex, Endiannesses.System);
            return CheckNumeric<TNumericType>(receivedValue, itemName, expectedValue);
        }

        //protected int CheckNumericUshort(byte[] fullArray, int startIndex, string itemName, ushort expectedValue)
        //{
        //    return CheckNumeric(fullArray, startIndex, itemName, expectedValue);
        //}

        //protected int CheckNumericUint(byte[] fullArray, int startIndex, string itemName, uint expectedValue)
        //{
        //    return CheckNumeric(fullArray, startIndex, itemName, expectedValue);
        //}

        //protected int CheckNumericUlong(byte[] fullArray, int startIndex, string itemName, ulong expectedValue)
        //{
        //    return CheckNumeric(fullArray, startIndex, itemName, expectedValue);
        //}

        //protected int CheckNumericInt(byte[] fullArray, int startIndex, string itemName, int expectedValue)
        //{
        //    return CheckNumeric(fullArray, startIndex, itemName, expectedValue);
        //}

        protected int CheckNumericBcd<TNumericType>(byte[] fullArray, int startIndex, string itemName, TNumericType expectedValue) where TNumericType : struct
        {
            int length = (typeof(TNumericType).GetField("MaxValue").GetValue(null).ToString().Length + 1) / 2;
            return CheckNumericBcd<TNumericType>(fullArray, startIndex, length, itemName, expectedValue);
            //TNumericType receivedValue = ArrayOps.GetNumericBcd<TNumericType>(fullArray, startIndex);
            //return CheckNumeric<TNumericType>(receivedValue, itemName, expectedValue);
        }

        protected int CheckNumericBcd<TNumericType>(byte[] fullArray, int startIndex, int length, string itemName, TNumericType expectedValue) where TNumericType : struct
        {
            TNumericType receivedValue = ArrayOps.GetNumericBcd<TNumericType>(fullArray, startIndex, length);
            CheckNumeric<TNumericType>(receivedValue, itemName, expectedValue);
            return length;
        }

        protected int CheckMultiByteArray(byte[] fullArray, int startIndex, string itemName, byte[] expectedArray)
        {
            for (int index = 0; index < expectedArray.Length; ++index)
            {
                Assert.That(fullArray[startIndex + index], Is.EqualTo(expectedArray[index]), itemName + " at index " + index);
            }

            return expectedArray.Length;
        }

        protected int CheckStringMatches(byte[] fullArray, int startIndex, string itemName, string expectedString, int fieldLength = -1)
        {
            // fieldLength of -1 means that it should just be the length of the string
            if (fieldLength == -1)
                fieldLength = expectedString.Length;

            // Note that in theory we could do something where it gets cut off but we aren't really going for that in the unit tests
            Assert.That(expectedString.Length, Is.AtMost(fieldLength), "Expected String Length of " + expectedString.Length + " is greater than the field length: " + fieldLength);

            byte[] stringAsArray = Encoding.ASCII.GetBytes(expectedString);
            int nextIndex = CheckMultiByteArray(fullArray, startIndex, itemName, stringAsArray);
            if (nextIndex < fieldLength)
            {
                int numberOfNulls = fieldLength - nextIndex;
                // This will be an array of 0's
                byte[] nullArray = new byte[numberOfNulls];
                nextIndex += CheckMultiByteArray(fullArray, startIndex + nextIndex, itemName, nullArray);
            }

            return nextIndex;
        }

        // TODO: These functions are borrowed from SerializerBase.  Should have another class that they can call or inherit from
        // so don't have the duplicated code.  And then do some unit tests for them
        protected byte[] GetSubArray(byte[] sourceArray, int startIndex, int length, bool reverse = false)
        {
            byte[] destinationArray = new byte[length];
            Array.Copy(sourceArray, startIndex, destinationArray, 0, length);
            if (reverse)
                Array.Reverse(destinationArray);
            return destinationArray;
        }

        protected string GetHexStringFromArray(byte[] array, int startIndex, int length, bool reverse = false)
        {
            byte[] subArray = GetSubArray(array, startIndex, length, reverse);
            return BitConverter.ToString(subArray).Replace("-", "");
        }

        protected string GetHexStringFromByteArray(byte[] arrayToConvert, string separator)
        {
            string convertedString = BitConverter.ToString(arrayToConvert);
            if (separator != "-")
                convertedString = convertedString.Replace("-", separator);

            return convertedString;
        }
    }
}
