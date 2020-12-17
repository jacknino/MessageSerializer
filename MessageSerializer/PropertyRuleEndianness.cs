namespace MessageSerializer
{
    public class PropertyRuleEndianness : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            var messagePropertyAttribute = messageSerializedPropertyInfo.MessagePropertyAttribute;
            if (!messagePropertyAttribute.IsEndiannessSpecified)
            {
                if (classAttribute.EndiannessExplicitlySpecified)
                    messagePropertyAttribute.Endianness = classAttribute.Endianness;
                else if (serializationDefaults != null)
                    messagePropertyAttribute.Endianness = serializationDefaults.Endianness;
                else
                    messagePropertyAttribute.Endianness = Endiannesses.System;
            }
        }
    }
}
