using System;

namespace MessageSerializer
{
    public class PropertyRuleAuthenticationField : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (!messageSerializedPropertyInfo.ContainsAuthenticationAttribute
                && messageSerializedPropertyInfo.PropertyInfo.Name.StartsWith("Crc", StringComparison.InvariantCultureIgnoreCase))
            {
                messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationCrc16)));
            }

            // By default authentication fields are excluded from length calculations
            if (!messageSerializedPropertyInfo.ContainsLengthAttribute && messageSerializedPropertyInfo.PropertyInfo.Name.StartsWith("Crc", StringComparison.InvariantCultureIgnoreCase))
                messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedLengthAttribute() { Exclude = true });
        }
    }
}
