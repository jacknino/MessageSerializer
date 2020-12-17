using System;

namespace MessageSerializer
{
    public class TypeSerializerBcd<TNumericType> : TypeSerializerBase<TNumericType> where TNumericType : struct, IComparable<TNumericType>, IConvertible
    {
        public TypeSerializerBcd(MessageSerializedPropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        // TODO: Find a better way to do this without all the ChangeTypes and whatnot
        public override byte[] Serialize(TNumericType value)
        {
            return GetBcdBytes(
                (ulong)Convert.ChangeType(value, typeof(ulong)),
                _propertyInfo.MessagePropertyAttribute.Length,
                _propertyInfo.MessagePropertyAttribute.MinLength,
                _propertyInfo.MessagePropertyAttribute.MaxLength,
                _propertyInfo.IsVariableLength && _propertyInfo.MessagePropertyAttribute.MinimizeVariableLength);
        }

        public override TNumericType Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            if (length == -1)
                length = GetLength();

            TNumericType returnValue = (TNumericType)Convert.ChangeType(GetValueFromBcdArray(bytes, currentArrayIndex, length), typeof(TNumericType));
            currentArrayIndex += length;
            return returnValue;
        }

        protected byte[] GetBcdBytes(ulong value, int length, int minLength = 0, int maxLength = -1, bool minimizeLength = false)
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
                byteArray = ArrayOps.GetSubArray(byteArray, arrayLength - bytesToTake, bytesToTake);
            }

            return byteArray;
        }

        // TODO: Change startIndex to ref currentIndex and increment (maybe)
        protected ulong GetValueFromBcdArray(byte[] bcdArray, int startIndex, int length)
        {
            byte[] subArray = ArrayOps.GetSubArray(bcdArray, startIndex, length);
            return GetValueFromBcdArray(subArray);
        }

        protected ulong GetValueFromBcdArray(byte[] bcdArray)
        {
            ulong value = 0;
            ulong multiplier = 1;
            for (int index = (bcdArray.Length - 1); index >= 0; --index)
            {
                value += (ulong)(bcdArray[index] & 0x0F) * multiplier;
                multiplier *= 10;
                value += (ulong)((bcdArray[index] >> 4) & 0x0F) * multiplier;
                multiplier *= 10;
            }

            return value;
        }
    }
}
