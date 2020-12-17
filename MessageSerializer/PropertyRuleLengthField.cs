namespace MessageSerializer
{
    public class PropertyRuleLengthField : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (!messageSerializedPropertyInfo.ContainsLengthAttribute
                && messageSerializedPropertyInfo.PropertyInfo.Name == "Length")
            {
                messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedLengthResultAttribute());
            }
        }
    }
}
