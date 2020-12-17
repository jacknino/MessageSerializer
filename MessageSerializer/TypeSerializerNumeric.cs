namespace MessageSerializer
{
    public class TypeSerializerNumeric<TNumericType> : TypeSerializerBase<TNumericType> where TNumericType : struct
    {
        public TypeSerializerNumeric(MessageSerializedPropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public override byte[] Serialize(TNumericType value)
        {
            return ArrayOps.GetBytesFromNumeric(value, _propertyInfo.MessagePropertyAttribute.Endianness);
        }

        public override TNumericType Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            if (length == -1)
                length = GetLength();

            TNumericType returnValue = ArrayOps.GetNumeric<TNumericType>(bytes, currentArrayIndex, length, _propertyInfo.MessagePropertyAttribute.Endianness);
            currentArrayIndex += length;
            return returnValue;
        }

        protected override string GetToStringValue(TNumericType value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)
        {
            return string.Format($"{{0}} (0x{{0:X{GetLength() * 2}}})", value);
        }
    }
}
