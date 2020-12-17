using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MessageSerializer
{
    public class MessageSerializedClassInfo
    {
        public Type ClassType { get; protected set; }
        public SerializationDefaults SerializationDefaults { get; protected set; }
        public MessageClassAttribute MessageClassAttribute { get; protected set; }
        public List<MessageSerializedPropertyInfo> Properties { get; protected set; }
        public List<CalculatedFieldInfo> CalculatedFields { get; protected set; }
        public SerializerBase Serializer { get; protected set; }
        public bool ContainsBlobData { get; protected set; }
        public int TotalLengthWithoutVariableData { get; protected set; }
        public bool IsVariableLength { get; protected set; }

        public MessageSerializedClassInfo(Type classType, List<ConfigMessageSerializerClass> configMessageSerializerClasses, SerializationDefaults serializationDefaults)
        {
            ClassType = classType;
            SerializationDefaults = serializationDefaults;
            GetClassInfo(configMessageSerializerClasses);
            SerializerClassGeneration serializerClassGeneration = new SerializerClassGeneration(serializationDefaults.TypeSelectors);
            Serializer = serializerClassGeneration.CreateSerializerClassForType(this);
        }

        public CalculatedFieldInfo GetCalculatedLengthInfo()
        {
            return CalculatedFields.First(item => item.CalculatorResultAttribute is CalculatedLengthResultAttribute);
        }

        protected void GetClassInfo(List<ConfigMessageSerializerClass> configMessageSerializerClasses)
        {
            MessageClassAttribute = GetMessageSerializedClassAttributeFromList(ClassType, configMessageSerializerClasses) ?? GetMessageSerializedClassAttribute();
            IsVariableLength = false;

            List<Type> types = GetTypesToIterate();

            int totalLengthWithoutVariableData = 0;
            int nonVaryingLengthPartOfMessageLength = 0;
            List<MessageSerializedPropertyInfo> messageSerializedProperties = new List<MessageSerializedPropertyInfo>();

            foreach (Type currentType in types)
            {
                ConfigClassInfo configClassInfo = GetConfigClassInfoFromList(currentType, configMessageSerializerClasses);
                List<MessageSerializedPropertyInfo> messageSerializedPropertiesForType = GetPropertiesForType(currentType, configClassInfo, ref totalLengthWithoutVariableData, ref nonVaryingLengthPartOfMessageLength);
                messageSerializedProperties.AddRange(messageSerializedPropertiesForType);
            }

            Properties = messageSerializedProperties;
            CalculatedFields = GetCalculatedFields();
            TotalLengthWithoutVariableData = totalLengthWithoutVariableData;
            AssociateBlobProperties();
        }

        protected List<Type> GetTypesToIterate()
        {
            List<Type> types = new List<Type>();
            Type currentType = ClassType;
            while (currentType != null)
            {
                types.Insert(MessageClassAttribute.PutInheritedPropertiesLast ? types.Count : 0, currentType);
                currentType = currentType.BaseType;
            }

            return types;
        }

        protected List<MessageSerializedPropertyInfo> GetPropertiesForType(Type type, ConfigClassInfo configClassInfo, ref int totalLengthWithoutVariableData, ref int nonVaryingLengthPartOfMessageLength)
        {
            PropertyInfo[] objectProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            List<MessageSerializedPropertyInfo> messageSerializedProperties = new List<MessageSerializedPropertyInfo>();
            int indexInMessage = 0;
            foreach (PropertyInfo propertyInfo in objectProperties)
            {
                MessagePropertyAttribute messagePropertyAttribute = 
                    GetMessageSerializedPropertyAttributeFromClassInfo(propertyInfo, configClassInfo, MessageClassAttribute.DefaultExcludeProperty) 
                    ?? GetMessageSerializedPropertyAttribute(propertyInfo, MessageClassAttribute.DefaultExcludeProperty);

                if (messagePropertyAttribute.Exclude)
                    continue;

                // Note: If you want to check something with the attribute you should use it after creating the MessageSerializablePropertyInfo
                // as that is what initializes any of the unset values
                MessageSerializedPropertyInfo messageSerializedPropertyInfo = new MessageSerializedPropertyInfo(indexInMessage++, propertyInfo, messagePropertyAttribute, SerializationDefaults, MessageClassAttribute);

                if (messagePropertyAttribute.MaxLength != -1 && messagePropertyAttribute.MaxLength < messagePropertyAttribute.MinLength)
                {
                    throw new Exception(string.Format("For {0} the MinLength ({1}) > MaxLength ({2}) which is not allowed",
                        messageSerializedPropertyInfo.PropertyInfo.Name, messageSerializedPropertyInfo.MessagePropertyAttribute.MinLength, messageSerializedPropertyInfo.MessagePropertyAttribute.MaxLength));
                }

                if (messageSerializedPropertyInfo.IsVariableLength)
                    IsVariableLength = true;

                int propertyLength = messageSerializedPropertyInfo.MessagePropertyAttribute.Length;
                if (messageSerializedPropertyInfo.MessagePropertyAttribute.BlobType == BlobTypes.Data)
                {
                    ContainsBlobData = true;
                }
                else if (!messageSerializedPropertyInfo.IsVariableLength)
                {
                    totalLengthWithoutVariableData += propertyLength;
                }

                messageSerializedProperties.Add(messageSerializedPropertyInfo);
            }

            return messageSerializedProperties;
        }

        protected void AssociateBlobProperties()
        {
            for (int index = 0; index < Properties.Count; ++index)
            {
                MessageSerializedPropertyInfo currentPropertyInfo = Properties[index];
                BlobTypes currentBlobType = currentPropertyInfo.MessagePropertyAttribute.BlobType;
                if (currentBlobType == BlobTypes.Length)
                {
                    // If an associated blob property is specified we just want to find that one after this one and make sure it doesn't have a different association and assign it.
                    // If there isn't one specified we just want to find the next blob field and assign that one.
                    string lengthAssociatedBlobProperty = currentPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty;
                    if (string.IsNullOrEmpty(lengthAssociatedBlobProperty))
                    {
                        for (int blobDataIndex = index + 1; blobDataIndex < Properties.Count; ++blobDataIndex)
                        {
                            MessageSerializedPropertyInfo blobDataPropertyInfo = Properties[blobDataIndex];
                            if (blobDataPropertyInfo.MessagePropertyAttribute.BlobType == BlobTypes.Data)
                            {
                                string dataAssociatedBlobProperty = blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty;
                                if (!string.IsNullOrEmpty(dataAssociatedBlobProperty)
                                    && dataAssociatedBlobProperty != currentPropertyInfo.PropertyInfo.Name)
                                {
                                    throw new Exception(string.Format("Blob Length Property {0} with no explicit association should have Blob Data Property {1} as its associated blob data but {1} has {2} as its associated length field", currentPropertyInfo.PropertyInfo.Name, blobDataPropertyInfo.PropertyInfo.Name, dataAssociatedBlobProperty));
                                }

                                blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty = currentPropertyInfo.PropertyInfo.Name;
                                currentPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty = blobDataPropertyInfo.PropertyInfo.Name;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(currentPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty))
                            throw new Exception(string.Format("Couldn't find an associated blob data field for blob length field {0}", currentPropertyInfo.PropertyInfo.Name));
                    }
                    else
                    {
                        for (int blobDataIndex = index + 1; blobDataIndex < Properties.Count; ++blobDataIndex)
                        {
                            MessageSerializedPropertyInfo blobDataPropertyInfo = Properties[blobDataIndex];
                            if (blobDataPropertyInfo.PropertyInfo.Name == lengthAssociatedBlobProperty)
                            {
                                if (blobDataPropertyInfo.MessagePropertyAttribute.BlobType != BlobTypes.Data)
                                    throw new Exception(string.Format("Blob Length Property {0} had {1} as its associated blob data property but {1} is not marked as a Blob Data field", currentPropertyInfo.PropertyInfo.Name, blobDataPropertyInfo.PropertyInfo.Name));

                                string dataAssociatedBlobProperty = blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty;
                                if (!string.IsNullOrEmpty(dataAssociatedBlobProperty)
                                    && dataAssociatedBlobProperty != currentPropertyInfo.PropertyInfo.Name)
                                {
                                    throw new Exception(string.Format("Blob Length Property {0} had an explicit association with Blob Data Property {1} as its associated blob data but {1} has {2} as its associated length field", currentPropertyInfo.PropertyInfo.Name, blobDataPropertyInfo.PropertyInfo.Name, dataAssociatedBlobProperty));
                                }

                                blobDataPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty = currentPropertyInfo.PropertyInfo.Name;
                                break;
                            }
                        }

                        if (string.IsNullOrEmpty(lengthAssociatedBlobProperty))
                            throw new Exception(string.Format("Couldn't find the explicity specified associated blob data property for blob length field {0}", lengthAssociatedBlobProperty, currentPropertyInfo.PropertyInfo.Name));
                    }
                }
                else if (currentBlobType == BlobTypes.Data)
                {
                    // Since all length fields should be before their associated data fields an empty associated field here means something is wrong
                    if (string.IsNullOrEmpty(currentPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty))
                        throw new Exception(string.Format("Blob data property {0} has no associated length field", currentPropertyInfo.PropertyInfo.Name));

                    // Now make sure the length field has the same association
                    string blobDataAssociatedProperty = currentPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty;
                    for (int blobDataIndex = 0; blobDataIndex < index; ++blobDataIndex)
                    {
                        MessageSerializedPropertyInfo blobLengthPropertyInfo = Properties[blobDataIndex];
                        if (blobLengthPropertyInfo.PropertyInfo.Name == blobDataAssociatedProperty)
                        {
                            if (blobLengthPropertyInfo.MessagePropertyAttribute.BlobType != BlobTypes.Length)
                                throw new Exception(string.Format("Blob Data Property {0} had {1} specified as its associated length property but {1} is not a blob length property", currentPropertyInfo.PropertyInfo.Name, blobDataAssociatedProperty));

                            if (blobLengthPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty != currentPropertyInfo.PropertyInfo.Name)
                                throw new Exception(string.Format("Blob Data Property {0} had {1} specified as its associated length property but {1} has {2} specified as its associated blob data", currentPropertyInfo.PropertyInfo.Name, blobDataAssociatedProperty, blobLengthPropertyInfo.MessagePropertyAttribute.AssociatedBlobProperty));
                        }
                    }
                }
            }
        }

        protected MessageClassAttribute GetMessageSerializedClassAttribute()
        {
            object[] messageSerializedClassAttributes = ClassType.GetCustomAttributes(typeof(MessageClassAttribute), false);
            if (messageSerializedClassAttributes.Length == 0)
            {
                return new MessageClassAttribute();
            }

            return (MessageClassAttribute)messageSerializedClassAttributes[0];
        }

        protected MessagePropertyAttribute GetMessageSerializedPropertyAttribute(PropertyInfo propertyInfo, bool defaultExcludeProperty)
        {
            object[] messageSerializedPropertyAttributes = propertyInfo.GetCustomAttributes(typeof(MessagePropertyAttribute), true);
            if (messageSerializedPropertyAttributes.Length == 0)
            {
                return GetDefaultMessageSerializedPropertyAttribute(defaultExcludeProperty);
            }

            return (MessagePropertyAttribute)messageSerializedPropertyAttributes[0];
        }

        protected ConfigClassInfo GetConfigClassInfoFromList(Type classType, List<ConfigMessageSerializerClass> configMessageSerializerClasses)
        {
            if (configMessageSerializerClasses == null)
                return null;

            foreach (ConfigMessageSerializerClass configMessageSerializerClass in configMessageSerializerClasses)
            {
                if (configMessageSerializerClass.ClassInfo.AssemblyQualifiedName == classType.AssemblyQualifiedName)
                    return configMessageSerializerClass.ClassInfo;
            }

            return null;
        }

        protected MessageClassAttribute GetMessageSerializedClassAttributeFromList(Type classType, List<ConfigMessageSerializerClass> configMessageSerializerClasses)
        {
            ConfigClassInfo configClassInfo = GetConfigClassInfoFromList(classType, configMessageSerializerClasses);
            return configClassInfo?.MessageClassAttribute;
        }

        protected MessagePropertyAttribute GetMessageSerializedPropertyAttributeFromClassInfo(PropertyInfo propertyInfo, ConfigClassInfo configClassInfo, bool defaultExcludeProperty)
        {
            if (configClassInfo == null)
                return null;

            foreach (ConfigPropertyInfo configPropertyInfo in configClassInfo.Properties)
            {
                if (configPropertyInfo.Name == propertyInfo.Name)
                    return (MessagePropertyAttribute)configPropertyInfo.Attributes.FirstOrDefault(item => item is MessagePropertyAttribute) ?? new MessagePropertyAttribute();
            }

            return GetDefaultMessageSerializedPropertyAttribute(defaultExcludeProperty);
        }

        protected MessagePropertyAttribute GetDefaultMessageSerializedPropertyAttribute(bool defaultExcludeProperty)
        {
            MessagePropertyAttribute messagePropertyAttribute = new MessagePropertyAttribute();
            messagePropertyAttribute.Exclude = defaultExcludeProperty;
            return messagePropertyAttribute;
        }

        protected List<CalculatedFieldInfo> GetCalculatedFields()
        {
            // First find all the calculated fields by name
            var calculatedFieldNames = new HashSet<string>();
            foreach (MessageSerializedPropertyInfo propertyInfo in Properties)
            {
                foreach (CalculatedFieldAttribute calculatedField in propertyInfo.CalculatedFieldAttributes)
                {
                    if (calculatedField is CalculatedFieldResultAttribute && !calculatedFieldNames.Contains(calculatedField.Name))
                        calculatedFieldNames.Add(calculatedField.Name);
                }
            }

            // Now figure out the actual information for each of the calculated fields
            var calculatedFields = new List<CalculatedFieldInfo>();
            foreach (string calculatedFieldName in calculatedFieldNames)
            {
                calculatedFields.Add(new CalculatedFieldInfo(calculatedFieldName, Properties));
            }

            // We want to sort the calculated fields by priority
            calculatedFields.Sort((x, y) => x.CalculatorResultAttribute.Priority.CompareTo(y.CalculatorResultAttribute.Priority));
            return calculatedFields;
        }

        public string ToString(int indentLevel)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indentLevel++.GetIndent() + $"Serialization class info for type {ClassType.PrintableType()} ({ClassType.FullName})");
            sb.AppendLine(indentLevel.GetIndent() + $"MessageClassAttribute: {MessageClassAttribute}");
            sb.AppendLine(indentLevel.GetIndent() + $"ContainsBlobData: {ContainsBlobData}, IsVariableLength: {IsVariableLength}, TotalLengthWithoutVariableData: {TotalLengthWithoutVariableData}");
            sb.AppendLine(indentLevel.GetIndent() + $"Properties ({Properties.Count}):");
            foreach (MessageSerializedPropertyInfo property in Properties)
            {
                sb.Append(property.ToString(indentLevel + 1));
            }
            sb.AppendLine(indentLevel.GetIndent() + $"CalculatedFields ({CalculatedFields.Count}):");
            foreach (CalculatedFieldInfo calculatedFieldInfo in CalculatedFields)
            {
                sb.Append(calculatedFieldInfo.ToString(indentLevel + 1));
                // Right now the CalculatedFieldInfo doesn't know the names of the field indexes it uses so we have to figure them out here
                sb.AppendLine((indentLevel + 2).GetIndent() + $"Applicable Fields: {string.Join(", ", calculatedFieldInfo.IncludedPropertyIndexes.Select(index => Properties[index].PropertyInfo.Name))}");
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }
    }
}
