using System;
using System.Collections.Generic;
using System.Reflection;

namespace MessageSerializer
{
    public class Serializer
    {
        private static readonly Serializer _instance = new Serializer();
        private readonly object _lock = new object();
        private readonly Dictionary<string, MessageSerializedClassInfo> _classInfos;

        protected Serializer()
        {
            _classInfos = new Dictionary<string, MessageSerializedClassInfo>();
        }

        public static Serializer Instance
        {
            get { return _instance; }
        }

        public byte[] Serialize<T>(T objectToSerialize)
            where T : class, IMessageSerializable
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(typeof(T));
            return classInfo.Serializer.Serialize(objectToSerialize);
        }

        public T Deserialize<T>(byte[] bytes)
            where T : class, IMessageSerializable
        {
            int currentArrayIndex = 0;
            DeserializeResults<T> results = DeserializeEx<T>(bytes, ref currentArrayIndex, false);
            return results.Object;
        }

        public T Deserialize<T>(byte[] bytes, ref int currentArrayIndex)
            where T : class, IMessageSerializable
        {
            DeserializeResults<T> results = DeserializeEx<T>(bytes, ref currentArrayIndex, false);
            return results.Object;
        }

        public DeserializeResults<T> DeserializeEx<T>(byte[] bytes)
            where T : class, IMessageSerializable
        {
            int currentArrayIndex = 0;
            return DeserializeEx<T>(bytes, ref currentArrayIndex, true);
        }

        public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, bool suppressExceptions)
            where T : class, IMessageSerializable
        {
            int currentArrayIndex = 0;
            return DeserializeEx<T>(bytes, ref currentArrayIndex, suppressExceptions);
        }

        public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, ref int currentArrayIndex)
            where T : class, IMessageSerializable
        {
            return DeserializeEx<T>(bytes, ref currentArrayIndex, true);
        }

        public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, ref int currentArrayIndex, bool suppressExceptions)
            where T : class, IMessageSerializable
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(typeof(T));
            return classInfo.Serializer.Deserialize<T>(bytes, ref currentArrayIndex, suppressExceptions);
        }

        public string ToString(IMessageSerializable objectToPrint, ToStringFormatProperties formatProperties = null)
        {
            return ToString(objectToPrint, true, 0, null, null, false, formatProperties);
        }

        public string ToString(IMessageSerializable objectToPrint, int indentLevel, ToStringFormatProperties formatProperties = null)
        {
            return ToString(objectToPrint, true, indentLevel, null, null, false, formatProperties);
        }

        public string ToString(IMessageSerializable objectToPrint, bool includeBytes, int indentLevel = 0, string separator = null, string bytesSeparator = null, bool putBytesAfter = false, ToStringFormatProperties formatProperties = null)
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(objectToPrint.GetType());

            string stringResult = string.Empty;
            if ((includeBytes && (putBytesAfter == false)))
            {
                stringResult += ArrayOps.GetHexStringFromByteArray(classInfo.Serializer.Serialize(objectToPrint), bytesSeparator) + separator;
            }

            stringResult += classInfo.Serializer.ToString(
                objectToPrint,
                indentLevel,
                formatProperties ?? ToStringFormatProperties.Default);

            if ((includeBytes && putBytesAfter))
            {
                stringResult += separator + ArrayOps.GetHexStringFromByteArray(classInfo.Serializer.Serialize(objectToPrint), bytesSeparator);
            }

            return stringResult;
        }

        public string GetClassInfoString(Type type, int indentLevel = 0)
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(type);
            return classInfo.ToString(indentLevel);
        }

        public string GetClassInfoString(IMessageSerializable objectForInfo, int indentLevel = 0)
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(objectForInfo.GetType());
            return classInfo.ToString(indentLevel);
        }

        public int GetFixedLength<T>()
        {
            MessageSerializedClassInfo classInfo = GetClassInfo(typeof(T));
            return classInfo.TotalLengthWithoutVariableData;
        }

        public void LoadSerializableClassesFromAssembly(Assembly assembly)
        {
            LoadSerializableClassesFromAssembly(assembly, false);
        }

        public void LoadSerializableClassesFromAssembly(Assembly assembly, bool replaceIfExists)
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (Type interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == typeof(IMessageSerializable))
                    {
                        GetClassInfo(type, replaceIfExists);
                        break;
                    }
                }
            }
        }

        public void LoadSerializableClassesFromSettings(List<ConfigMessageSerializerClass> configMessageSerializerClasses, SerializationDefaults serializationDefaults = null)
        {
            LoadSerializableClassesFromSettings(configMessageSerializerClasses, false, serializationDefaults);
        }

        public void LoadSerializableClassesFromSettings(List<ConfigMessageSerializerClass> configMessageSerializerClasses, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
        {
            foreach (ConfigMessageSerializerClass configMessageSerializerClass in configMessageSerializerClasses)
            {
                Type type = Type.GetType(configMessageSerializerClass.ClassInfo.AssemblyQualifiedName);
                if (type == null)
                    throw new Exception(string.Format("Couldn't find type {0} to create serializer class", configMessageSerializerClass.ClassInfo.AssemblyQualifiedName));
                GetClassInfo(type, configMessageSerializerClasses, replaceIfExists);
            }
        }

        public MessageSerializedClassInfo GetClassInfo(Type type, SerializationDefaults serializationDefaults = null)
        {
            return GetClassInfo(type, false, serializationDefaults);
        }

        public MessageSerializedClassInfo GetClassInfo(Type type, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
        {
            return GetClassInfo(type, null, replaceIfExists, serializationDefaults);
        }

        public MessageSerializedClassInfo GetClassInfo(Type type, List<ConfigMessageSerializerClass> configMessageSerializerClasses, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
        {
            lock (_lock)
            {
                if (_classInfos.ContainsKey(type.FullName))
                {
                    if (!replaceIfExists)
                        return _classInfos[type.FullName];

                    _classInfos.Remove(type.FullName);
                }

                if (serializationDefaults == null)
                    serializationDefaults = new SerializationDefaults();
                MessageSerializedClassInfo messageSerializedClassInfo = new MessageSerializedClassInfo(type, configMessageSerializerClasses, serializationDefaults);
                _classInfos.Add(type.FullName, messageSerializedClassInfo);

                return messageSerializedClassInfo;
            }
        }

        public ConfigMessageSerializerClass CreateClassInfoFromType<T>()
        {
            return CreateClassInfoFromType(typeof(T));
        }

        public ConfigMessageSerializerClass CreateClassInfoFromType(Type type, SerializationDefaults serializationDefaults = null)
        {
            if (serializationDefaults == null)
                serializationDefaults = new SerializationDefaults();
            MessageSerializedClassInfo messageSerializedClassInfo = new MessageSerializedClassInfo(type, null, serializationDefaults);

            ConfigMessageSerializerClass configMessageSerializerClass = new ConfigMessageSerializerClass();
            ConfigClassInfo configClassInfo = new ConfigClassInfo();
            configClassInfo.AssemblyName = messageSerializedClassInfo.ClassType.Assembly.FullName;
            configClassInfo.ClassFullName = messageSerializedClassInfo.ClassType.FullName;
            configClassInfo.MessageClassAttribute = messageSerializedClassInfo.MessageClassAttribute;
            configMessageSerializerClass.ClassInfo = configClassInfo;

            foreach (MessageSerializedPropertyInfo messageSerializedPropertyInfo in messageSerializedClassInfo.Properties)
            {
                ConfigPropertyInfo configPropertyInfo = new ConfigPropertyInfo();
                configPropertyInfo.Name = messageSerializedPropertyInfo.PropertyInfo.Name;
                //configPropertyInfo._messagePropertyAttribute = messageSerializedPropertyInfo.MessagePropertyAttribute;
                configPropertyInfo.Attributes.Add(messageSerializedPropertyInfo.MessagePropertyAttribute);
                configClassInfo.Properties.Add(configPropertyInfo);
            }

            return configMessageSerializerClass;
        }
    }
}
