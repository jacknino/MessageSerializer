using System;

namespace MessageSerializer
{
    public class PropertyRuleDateTime : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (messageSerializedPropertyInfo.ElementType.FullName == typeof(DateTime).FullName)
            {
                var messagePropertyAttribute = messageSerializedPropertyInfo.MessagePropertyAttribute;
                if (!messagePropertyAttribute.IsIsBcdSpecified)
                    messagePropertyAttribute.IsBcd = true;

                if (!messagePropertyAttribute.IsFormatSpecified)
                    messagePropertyAttribute.Format = "MMddyyyyHHmmss";

                if (!messagePropertyAttribute.IsLengthSpecified)
                    messagePropertyAttribute.Length = messagePropertyAttribute.Format.Length / 2; // BCD 
            }
        }
    }
}
