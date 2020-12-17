using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MessageSerializer
{
    public class MessageSerializedPropertyInfo
    {
        public int Index { get; protected set; }
        public PropertyInfo PropertyInfo { get; protected set; }
        public MessagePropertyAttribute MessagePropertyAttribute { get; protected set; }
        public List<CalculatedFieldAttribute> CalculatedFieldAttributes { get; protected set; }
        public Type ElementType { get; protected set; }
        public bool ElementIsMessageSerializableObject { get; protected set; }
        public bool IsList { get; protected set; }

        public MessageSerializedPropertyInfo(int index, PropertyInfo propertyInfo, MessagePropertyAttribute messagePropertyAttribute, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            Index = index;
            PropertyInfo = propertyInfo;
            SetElementType(propertyInfo.PropertyType);
            ElementIsMessageSerializableObject = TypeIsMessageSerializableObject(ElementType);
            MessagePropertyAttribute = messagePropertyAttribute;
            CalculatedFieldAttributes = GetCalculatedFieldAttributes(propertyInfo);

            //TODO: throw new Exception("Probably need to change this to be called after all the MessageSerializedPropertyInfos are created and then pass the list in to CheckRules.  Also, since it's a member, a bunch of the parameters don't need to be passed in anyways");
            CheckRules(messagePropertyAttribute, propertyInfo, ElementType, ElementIsMessageSerializableObject, serializationDefaults, classAttribute);
        }

        protected void CheckRules(MessagePropertyAttribute messagePropertyAttribute, PropertyInfo propertyInfo, Type elementType, bool isMessageSerializable, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
        {
            if (serializationDefaults is null)
                return;

            messagePropertyAttribute.ApplyingRules = true;
            foreach (IPropertyRule propertyRule in serializationDefaults.PropertyRules)
            {
                propertyRule.Check(this, serializationDefaults, classAttribute);
            }

            messagePropertyAttribute.ApplyingRules = false;
        }

        public bool ContainsLengthAttribute
        {
            get { return CalculatedFieldAttributes.Any(item => item is CalculatedLengthAttribute || item is CalculatedLengthResultAttribute); }
        }

        public bool ContainsAuthenticationAttribute
        {
            get { return CalculatedFieldAttributes.Any(item => item is CalculatedAuthenticationAttribute || item is CalculatedAuthenticationResultAttribute); }
        }

        public bool IsVariableLength
        {
            get { return IsList || MessagePropertyAttribute.VariableLength; }
        }

        public bool IsCalculatedResult
        {
            get { return CalculatedFieldAttributes.Any(calculatedFieldAttribute => calculatedFieldAttribute is CalculatedFieldResultAttribute); }
        }

        protected void SetElementType(Type propertyType)
        {
            // If the propertyType is ICollection<T> then we want the element type
            // to be T and then the property is a list.
            // We can't just use IEnumerable because when deserializing we need
            // to be able to add an element
            // However, arrays are a special type that say they support ICollection<T>
            // but if you try and call Add they throw an exception so we exclude types
            // that say they are an array
            if (!propertyType.IsArray)
            {
                foreach (Type interfaceType in propertyType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType
                        && interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        ElementType = interfaceType.GetGenericArguments()[0];
                        IsList = true;
                        return;
                    }
                }
            }

            ElementType = propertyType;
        }

        protected bool TypeIsMessageSerializableObject(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType == typeof(IMessageSerializable))
                    return true;
            }

            return false;
        }

        protected List<CalculatedFieldAttribute> GetCalculatedFieldAttributes(PropertyInfo propertyInfo)
        {
            var calculatedFieldAttributes = (CalculatedFieldAttribute[])propertyInfo.GetCustomAttributes(typeof(CalculatedFieldAttribute), true);
            return calculatedFieldAttributes.ToList();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public string ToString(int indentLevel)
        {
            var sb = new StringBuilder();
            // List<int> Blah
            //    ElementType: int
            //    Attributes:
            //    MessagePropertyAttribute: Not Specified
            //    Calculated Fields (1):
            //        CalculatedLength: Exclude = true
            sb.AppendLine(indentLevel++.GetIndent() + $"{PropertyInfo.PropertyType.PrintableType()} {PropertyInfo.Name}:");
            sb.AppendLine(indentLevel.GetIndent() + $"IsList: {IsList}, ElementType: {ElementType.PrintableType()}");
            if (ElementIsMessageSerializableObject)
            {
                sb.AppendLine(indentLevel.GetIndent() + $"Element Is Message Serializable");
                sb.Append(Serializer.Instance.GetClassInfoString(ElementType, indentLevel + 1));
            }
            else
            {
                string messagePropertyAttributeString = MessagePropertyAttribute.ToString();
                if (!string.IsNullOrWhiteSpace(messagePropertyAttributeString) || CalculatedFieldAttributes.Count > 0)
                {
                    sb.AppendLine(indentLevel.GetIndent() + "Attributes:");
                    ++indentLevel;

                    if (!string.IsNullOrWhiteSpace(messagePropertyAttributeString))
                        sb.AppendLine(indentLevel.GetIndent() + messagePropertyAttributeString);

                    if (CalculatedFieldAttributes.Count > 0)
                    {
                        sb.AppendLine(indentLevel.GetIndent() + $"CalculatedFieldAttributes: {CalculatedFieldAttributes.Count}");
                        foreach (CalculatedFieldAttribute calculatedFieldAttribute in CalculatedFieldAttributes)
                            sb.AppendLine((indentLevel + 1).GetIndent() + $"{calculatedFieldAttribute}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
