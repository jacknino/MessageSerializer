using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MessageSerializer
{
    public class ArrayOps
    {
        // Unfortunately, even though there is a Convert.ChangeType that wouldn't require passing in the parse function
        // it can't take hex string so we are pretty much stuck with int.Parse, ushort.Parse, etc.
        protected static Dictionary<Type, Func<string, NumberStyles, object>> ParsingDictionary = new Dictionary<Type, Func<string, NumberStyles, object>>
        {
            { typeof(byte), (s, n) => byte.Parse(s, n) },
            { typeof(sbyte), (s, n) => sbyte.Parse(s, n) },
            { typeof(short), (s, n) => short.Parse(s, n) },
            { typeof(ushort), (s, n) => ushort.Parse(s, n) },
            { typeof(int), (s, n) => int.Parse(s, n) },
            { typeof(uint), (s, n) => uint.Parse(s, n) },
            { typeof(long), (s, n) => long.Parse(s, n) },
            { typeof(ulong), (s, n) => ulong.Parse(s, n) }
        };

        protected static Dictionary<Type, Func<object, byte[]>> GetBytesDictionary = new Dictionary<Type, Func<object, byte[]>>
        {
            // NOTE: BitConvert.GetBytes doesn't work for 1-byte types
            { typeof(byte), (value) => new byte[] { (byte)value } },
            { typeof(sbyte), (value) => Array.ConvertAll(new sbyte[] { (sbyte)value }, (a) => (byte)a) },
            { typeof(short), (value) => BitConverter.GetBytes((short)value) },
            { typeof(ushort), (value) => BitConverter.GetBytes((ushort)value) },
            { typeof(int), (value) => BitConverter.GetBytes((int)value) },
            { typeof(uint), (value) => BitConverter.GetBytes((uint)value) },
            { typeof(long), (value) => BitConverter.GetBytes((long)value) },
            { typeof(ulong), (value) => BitConverter.GetBytes((ulong)value) }
        };

        protected static Dictionary<Type, Func<byte[], object>> GetNumericFromBytesDictionary = new Dictionary<Type, Func<byte[], object>>
        {
            // NOTE: BitConvert.GetBytes doesn't work for 1-byte types
            { typeof(byte), (value) => value[0] },
            { typeof(sbyte), (value) => (Array.ConvertAll(value, (a) => (sbyte)a))[0] },
            { typeof(short), (value) => BitConverter.ToInt16(value, 0) },
            { typeof(ushort), (value) => BitConverter.ToUInt16(value, 0) },
            { typeof(int), (value) => BitConverter.ToInt32(value, 0) },
            { typeof(uint), (value) => BitConverter.ToUInt32(value, 0) },
            { typeof(long), (value) => BitConverter.ToInt64(value, 0) },
            { typeof(ulong), (value) => BitConverter.ToUInt64(value, 0) }
        };

        public static bool SystemEndiannessIsLittleEndian()
        {
            return BitConverter.IsLittleEndian;
        }

        public static bool EndiannessRequiresReversal(Endiannesses requiredEndianness)
        {
            // We need to reverse the byte arrays if the endianness of the bytes is different from the system
            // Specified | BitConverter.IsLittleEndian | EndiannessRequiresReversal
            // System    | N/A                         | false
            // Little    | true                        | false
            // Little    | false                       | true
            // Big       | true                        | true
            // Big       | false                       | false
            //return (requiredEndianness == Endiannesses.Little && !BitConverter.IsLittleEndian)
            //    || (requiredEndianness == Endiannesses.Big && BitConverter.IsLittleEndian);
            return EndiannessRequiresReversal(Endiannesses.System, requiredEndianness);
        }

        public static bool EndiannessRequiresReversal(Endiannesses currentEndianness, Endiannesses requiredEndianness)
        {
            // We need to reverse the byte arrays if the required endianness of the bytes is different from
            // whatever order they are currently in
            // Current   | Required | BitConverter.IsLittleEndian | EndiannessRequiresReversal
            // System    | System   | N/A                         | false
            // System    | Little   | true                        | false
            // System    | Little   | false                       | true
            // System    | Big      | true                        | true
            // System    | Big      | false                       | false
            // Little    | System   | true                        | false
            // Little    | System   | false                       | true
            // Little    | Little   | N/A                         | false
            // Little    | Big      | N/A                         | true
            // Big       | System   | true                        | true
            // Big       | System   | false                       | false
            // Big       | Little   | N/A                         | true
            // Big       | Big      | N/A                         | false
            switch (currentEndianness)
            {
            case Endiannesses.Little:
                return requiredEndianness == Endiannesses.Big || (requiredEndianness == Endiannesses.System && !BitConverter.IsLittleEndian);

            case Endiannesses.Big:
                return requiredEndianness == Endiannesses.Little || (requiredEndianness == Endiannesses.System && BitConverter.IsLittleEndian);

            default: // System
                return (requiredEndianness == Endiannesses.Little && !BitConverter.IsLittleEndian)
                    || (requiredEndianness == Endiannesses.Big && BitConverter.IsLittleEndian);
            }
        }

        public static TNumericType Parse<TNumericType>(string stringValue, NumberStyles numberStyle = NumberStyles.None)
        {
            Func<string, NumberStyles, object> parseFunction = ParsingDictionary[typeof(TNumericType)];
            return (TNumericType)parseFunction(stringValue, numberStyle);
        }

        public static byte[] GetBytes<TNumericType>(TNumericType value)
        {
            Func<object, byte[]> getBytesFunction = GetBytesDictionary[typeof(TNumericType)];
            return getBytesFunction(value);
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] combinedArray = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, combinedArray, offset, array.Length);
                offset += array.Length;
            }
            return combinedArray;
        }

        public static byte[] GetSubArray(byte[] sourceArray, int startIndex, int length, bool reverse = false)
        {
            byte[] destinationArray = new byte[length];
            Array.Copy(sourceArray, startIndex, destinationArray, 0, length);
            if (reverse)
                Array.Reverse(destinationArray);
            return destinationArray;
        }

        public static string GetHexStringFromArray(byte[] array, int startIndex, int length, bool reverse = false)
        {
            byte[] subArray = GetSubArray(array, startIndex, length, reverse);
            return BitConverter.ToString(subArray).Replace("-", "");
        }

        public static string GetHexStringFromByteArray(byte[] arrayToConvert, string separator)
        {
            string convertedString = BitConverter.ToString(arrayToConvert);
            if (separator != "-")
                convertedString = convertedString.Replace("-", separator);

            return convertedString;
        }

        // TODO: Later .Net allows using Enum as a constraint
        public static byte[] GetBytesFromEnum<TEnumType>(TEnumType value, Endiannesses requiredEndianness) where TEnumType : struct, IConvertible
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnumType));
            Func<object, byte[]> getBytesFunction = GetBytesDictionary[underlyingType];
            byte[] byteArray = getBytesFunction(value);
            if (EndiannessRequiresReversal(requiredEndianness))
                Array.Reverse(byteArray);

            return byteArray;
        }

        public static byte[] GetBytesFromNumeric<TNumericType>(TNumericType value, Endiannesses requiredEndianness) where TNumericType : struct
        {
            Func<object, byte[]> getBytesFunction = GetBytesDictionary[typeof(TNumericType)];
            byte[] byteArray = getBytesFunction(value);
            if (EndiannessRequiresReversal(requiredEndianness))
                Array.Reverse(byteArray);

            return byteArray;
        }

        public static TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness, Func<byte[], object> parseFunction) where TNumericType : struct
        {
            //return GetNumeric<TNumericType>(fullArray, startIndex, length, EndiannessRequiresReversal(currentEndianness), parseFunction);
            byte[] subArray = GetSubArray(fullArray, startIndex, length, EndiannessRequiresReversal(currentEndianness));
            TNumericType numericValue = (TNumericType)parseFunction(subArray);

            return numericValue;
        }

        //public static TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, bool requiresReversal, Func<byte[], object> parseFunction) where TNumericType : struct
        //{
        //    byte[] subArray = GetSubArray(fullArray, startIndex, length, requiresReversal);
        //    TNumericType numericValue = (TNumericType)parseFunction(subArray);

        //    return numericValue;
        //}

        public static TEnumType GetEnum<TEnumType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness) where TEnumType : struct, IConvertible
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnumType));
            Func<byte[], object> parseFunction = GetNumericFromBytesDictionary[underlyingType];
            return GetNumeric<TEnumType>(fullArray, startIndex, length, currentEndianness, parseFunction);
        }

        public static TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness) where TNumericType : struct
        {
            Func<byte[], object> parseFunction = GetNumericFromBytesDictionary[typeof(TNumericType)];
            return GetNumeric<TNumericType>(fullArray, startIndex, length, currentEndianness, parseFunction);
        }

        //public static TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, bool requiresReversal) where TNumericType : struct
        //{
        //    Func<byte[], object> parseFunction = GetNumericFromBytesDictionary[typeof(TNumericType)];
        //    return GetNumeric<TNumericType>(fullArray, startIndex, length, requiresReversal, parseFunction);
        //}

        public static TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, Endiannesses currentEndianness) where TNumericType : struct
        {
            int length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TNumericType));
            return GetNumeric<TNumericType>(fullArray, startIndex, length, currentEndianness);
        }

        public static TNumericType GetNumeric<TNumericType>(byte[] array, Endiannesses currentEndianness) where TNumericType : struct
        {
            return GetNumeric<TNumericType>(array, 0, array.Length, currentEndianness);
        }

        public static ulong GetNumericBcdAsUlong(byte[] bcdArray, int startIndex, int length)
        {
            ulong value = 0;
            ulong multiplier = 1;
            for (int index = ((startIndex + length) - 1); index >= startIndex; --index)
            {
                value += (ulong)(bcdArray[index] & 0x0F) * multiplier;
                multiplier *= 10;
                value += (ulong)((bcdArray[index] >> 4) & 0x0F) * multiplier;
                multiplier *= 10;
            }

            return value;
        }

        public static ulong GetNumericBcdAsUlong(byte[] bcdArray)
        {
            return GetNumericBcdAsUlong(bcdArray, 0, bcdArray.Length);
        }

        public static TNumericType GetNumericBcd<TNumericType>(byte[] fullArray, int startIndex, int length) where TNumericType : struct
        {
            ulong ulongValue = GetNumericBcdAsUlong(fullArray, startIndex, length);
            return (TNumericType)Convert.ChangeType(ulongValue, typeof(TNumericType));
        }

        public static TNumericType GetNumericBcd<TNumericType>(byte[] fullArray, int startIndex) where TNumericType : struct
        {
            int length = System.Runtime.InteropServices.Marshal.SizeOf(typeof(TNumericType));
            return GetNumericBcd<TNumericType>(fullArray, startIndex, length);
        }

        public static string GetStringFromByteArray(byte[] sourceArray, int startIndex = 0, int count = -1)
        {
            // if count == -1 we want to go from startIndex to the end of the array
            if (count == -1)
                count = sourceArray.Length - startIndex;

            // NOTE: C# strings can happily contain null characters
            // Often when they are displayed (like in a UI) the display will stop at the null
            // This can cause confusing problems because two strings that look the same aren't
            // So we need to make sure that we stop at the null if there is one
            int nullIndex = Array.IndexOf<byte>(sourceArray, 0, startIndex, count);
            if (nullIndex != -1)
            {
                // So if we have a 20 byte array and we are looking at startIndex 10 and count of 10
                // if we get say 17 back from this, this means that rather than count being 10 we only
                // want it to be 7 (elements 10-16).  The nullIndex returned is the index from 0 of the
                // the array, not the index from startIndex.  So (nullIndex - startIndex) gives us how
                // how many characters we want to exclude the nulls
                count = nullIndex - startIndex;
            }

            return Encoding.ASCII.GetString(sourceArray, startIndex, count);
        }

        public static byte[] GetBcdBytes(ulong value, int length, int minLength = 0, int maxLength = -1, bool minimizeLength = false)
        {
            // First we want the size to be the max of the length passed in and minLength
            // Then we want it to be the min of maxLength (if it's not -1) and length
            // At the end if minimizeLength is true and we haven't filled the full array
            // we just want to return the bytes we did use
            int arrayLength = Math.Max(length, minLength);
            if (maxLength != -1)
                arrayLength = Math.Min(arrayLength, maxLength);

            byte[] byteArray = new byte[arrayLength];

            int index = arrayLength - 1;
            while (value > 0 && index >= 0)
            {
                byte currentValue = (byte)(value % 10);
                value /= 10;
                currentValue += (byte)((value % 10) << 4);
                value /= 10;

                byteArray[index--] = currentValue;
            }

            if (minimizeLength && arrayLength > 0 && index >= 0)
            {
                int bytesToTake = Math.Max(minLength, (arrayLength - index) - 1);
                byteArray = GetSubArray(byteArray, arrayLength - bytesToTake, bytesToTake);
            }

            return byteArray;
        }
    }
}
