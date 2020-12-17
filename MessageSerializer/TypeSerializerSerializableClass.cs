namespace MessageSerializer
{
    public class TypeSerializerSerializableClass<TSerializableType> : TypeSerializerBase<TSerializableType> where TSerializableType : class, IMessageSerializable
    {
        public TypeSerializerSerializableClass(MessageSerializedPropertyInfo propertyInfo)
            : base(propertyInfo)
        {
        }

        public override byte[] Serialize(TSerializableType value)
        {
            return Serializer.Instance.Serialize(value);
        }

        public override TSerializableType Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
        {
            return Serializer.Instance.Deserialize<TSerializableType>(bytes, ref currentArrayIndex);
        }

        protected override string GetToStringValue(TSerializableType value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)
        {
            // It's kind of hard to explain but for subclasses (which is what we have if we are here),
            // if we are using separate lines for each field we actually want to have the first item
            // on a new line/indented even though normally the first item does not go on a new line
            // when doing a ToString.  This is basically to account for there being a line for the "header"
            // of the subclass which is the name of the property that holds the subclass
            // However, if the subclass is the type of a list (List<SubClass>) then the ListItems
            // format properties will take care of the new line
            return formatProperties.Fields.GetIndentString(formatProperties.Fields.GetNewIndentLevel(indentLevel, false), isPartOfList) +
                Serializer.Instance.ToString(value, false, indentLevel, null, null, false, formatProperties);
        }
    }
}
