using System;
using System.Runtime.InteropServices;

namespace MessageSerializer
{
    public class PropertyRuleLengths : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            var elementType = messageSerializedPropertyInfo.ElementType;
            var messagePropertyAttribute = messageSerializedPropertyInfo.MessagePropertyAttribute;
            if (!messagePropertyAttribute.IsLengthSpecified && NumericFunctions.IsIntegerType(elementType))
            {
                // If we are using BCD the max length isn't the number of bytes of the type,
                // it's however long the maximum number is.  Sort of.
                // As an example, the MaxUInt is 4294967295, which is 10 digits.
                // So really, it is 9 digits.  Of course that doesn't quite work
                // because that is 4 1/2 bytes so really we should support 5 bytes but limit
                // how many we fill in but for now we will just say it is the minimum number 
                // of bytes that things can safely be fit in.
                // elementType.GetField("MaxValue").GetValue(null).ToString()
                // The call to GetField uses Reflection to the get static MaxValue field.
                // The call to GetValue(null) gets the value of the MaxValue field (the null is because it's static)
                // Then the ToString is to figure out the number of digits that can be used.
                Type lengthType = elementType.IsEnum ? Enum.GetUnderlyingType(elementType) : elementType;
                messagePropertyAttribute.Length = messagePropertyAttribute.IsBcd ? (lengthType.GetField("MaxValue").GetValue(null).ToString().Length + 1) / 2 : Marshal.SizeOf(lengthType);
            }

            if (!messagePropertyAttribute.IsLengthSpecified && messageSerializedPropertyInfo.ElementIsMessageSerializableObject)
            {
                MessageSerializedClassInfo classInfo = Serializer.Instance.GetClassInfo(elementType);
                messagePropertyAttribute.Length = classInfo.TotalLengthWithoutVariableData;
            }

            if (!messagePropertyAttribute.IsVariableLengthSpecified && messagePropertyAttribute.BlobType == BlobTypes.Data)
                messagePropertyAttribute.VariableLength = true;

            if (!messagePropertyAttribute.IsVariableLengthSpecified && messagePropertyAttribute.Length == 0 && elementType.FullName == typeof(string).FullName)
                messagePropertyAttribute.VariableLength = true;

            if (!messagePropertyAttribute.IsMinLengthSpecified)
                messagePropertyAttribute.MinLength = 0;

            if (!messagePropertyAttribute.IsMaxLengthSpecified)
                messagePropertyAttribute.MaxLength = -1;

            if (!messagePropertyAttribute.IsMinimizeVariableLengthSpecified)
                messagePropertyAttribute.MinimizeVariableLength = false;
        }
    }
}
