namespace MessageSerializer
{
    public interface IPropertyRule
    {
        void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute);
    }
}
