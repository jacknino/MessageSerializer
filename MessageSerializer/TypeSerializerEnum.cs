using System;

namespace MessageSerializer
{
    public class TypeSerializerEnum<TEnumType> : TypeSerializerBase<TEnumType> where TEnumType : struct, IConvertible
    {
        // TODO: It would be nice to be able to declare a TypeSerializerNumeric using the UnderlyingType but that doesn't seem possible
        public TypeSerializerEnum(MessageSerializedPropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public override byte[] Serialize(TEnumType value)
        {
            return ArrayOps.GetBytesFromEnum(value, _propertyInfo.MessagePropertyAttribute.Endianness);
        }

        public override TEnumType Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            if (length == -1)
                length = GetLength();

            TEnumType returnValue = ArrayOps.GetEnum<TEnumType>(bytes, currentArrayIndex, length, _propertyInfo.MessagePropertyAttribute.Endianness);
            currentArrayIndex += length;
            return returnValue;
        }
    }
}
