namespace MessageSerializer
{
    public class PropertyRulePrepad : IPropertyRule
    {
        public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (!messageSerializedPropertyInfo.MessagePropertyAttribute.IsPrepadCharacterSpecified)
                messageSerializedPropertyInfo.MessagePropertyAttribute.PrepadCharacter = '0';
        }
    }
}
