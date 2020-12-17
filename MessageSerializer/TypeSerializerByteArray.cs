using System;

namespace MessageSerializer
{
    public class TypeSerializerByteArray : TypeSerializerBase<byte[]>
    {
        public TypeSerializerByteArray(MessageSerializedPropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public override byte[] Serialize(byte[] value)
        {
            // We need to make sure we actually have an array to deal with
            value = value ?? new byte[0];

            // If the field is fixed length then we want to make sure we return that many bytes
            // If the value passed in is too short then additional 0's will be added at the end
            // If the passed in value is too long then it will be cut off at Length bytes
            // For variable length fields we want to check the MinLength and MaxLength and make sure
            // the result matches those using the same rules.  Note that MinLength defaults to 0
            // and MaxLength defaults to -1 (meaning there isn't a max length)
            int maxBytesToGet = value.Length;
            int minBytesToReturn;
            if (_propertyInfo.IsVariableLength)
            {
                if (_propertyInfo.MessagePropertyAttribute.MaxLength != -1)
                    maxBytesToGet = Math.Min(maxBytesToGet, _propertyInfo.MessagePropertyAttribute.MaxLength);

                minBytesToReturn = _propertyInfo.MessagePropertyAttribute.MinLength;
            }
            else
            {
                minBytesToReturn = _propertyInfo.MessagePropertyAttribute.Length;
                maxBytesToGet = Math.Min(minBytesToReturn, value.Length);
            }

            byte[] serializedValue = ArrayOps.GetSubArray(value, 0, maxBytesToGet);
            if (serializedValue.Length < minBytesToReturn)
            {
                Array.Resize(ref serializedValue, minBytesToReturn);
            }

            return serializedValue;
        }

        public override TListType DeserializeList<TListType>(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            // If the list type is just a byte array we only need to do the regular byte array
            if (typeof(TListType) == typeof(byte[]))
                return (TListType)(object)Deserialize(bytes, ref currentArrayIndex, length, ref status);
            return base.DeserializeList<TListType>(bytes, ref currentArrayIndex, length, ref status);
        }

        public override byte[] Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            byte[] deserializedValue = ArrayOps.GetSubArray(bytes, currentArrayIndex, length);
            currentArrayIndex += length;
            return deserializedValue;
        }

        protected override string GetToStringValue(byte[] value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)
        {
            return BitConverter.ToString(value);
        }
    }
}
