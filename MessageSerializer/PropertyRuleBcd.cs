using System;

namespace MessageSerializer
{
    public class PropertyRuleBcd : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (!messageSerializedPropertyInfo.MessagePropertyAttribute.IsIsBcdSpecified && messageSerializedPropertyInfo.PropertyInfo.Name.StartsWith("Bcd", StringComparison.InvariantCultureIgnoreCase))
                messageSerializedPropertyInfo.MessagePropertyAttribute.IsBcd = true;
        }
    }
}
