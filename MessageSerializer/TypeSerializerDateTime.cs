using System;

namespace MessageSerializer
{
    public class TypeSerializerDateTime : TypeSerializerBase<DateTime>
    {
        public TypeSerializerDateTime(MessageSerializedPropertyInfo propertyInfo) 
            : base(propertyInfo)
        {
        }

        protected int GetOutputLength()
        {
            return (_propertyInfo.MessagePropertyAttribute.Format.Length + 1) / 2;
        }

        public override byte[] Serialize(DateTime value)
        {
            // TODO: Make this work without having to convert to a numeric or at the very least support longer values
            return ArrayOps.GetBcdBytes(System.Convert.ToUInt64(value.ToString(_propertyInfo.MessagePropertyAttribute.Format)), GetOutputLength());
        }

        public override DateTime Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            // TODO: Should the length parameter be checked against GetOutputLength?
            int outputLength = GetOutputLength();
            DateTime returnValue = DateTime.ParseExact(ArrayOps.GetHexStringFromArray(bytes, currentArrayIndex, outputLength), _propertyInfo.MessagePropertyAttribute.Format, null);
            currentArrayIndex += outputLength;
            return returnValue;
        }

        protected override string GetToStringValue(DateTime value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
