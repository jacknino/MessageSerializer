using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MessageSerializer
{
    // The way the MessagePropertyAttribute is being serialized is probably not the greatest thing ever
    // because the flags for whether certain values are set need to be declared at least twice, in a property
    // for actually keeping track of whether the value was actually specified and then in a function called:
    // ShouldSerializeNameOfProperty() which just returns whether the value was explicitly specified or not.
    // However, this does make it really clean as far as the MessagePropertyAttribute class can be used
    // both when attributes are being specified on a class and when being read from an XML file.
    // Another option was that the IsPropertyExplicitlySpecified could have been renamed to PropertySpecified
    // but then it would have had to have been made public (and marked XmlIgnore) and thus would have shown
    // up as an option when declaring a MessageSerializableProperty attribute.  Removing the setter for PropertySpecified
    // worked except then there isn't a way to set it unless a separate variable is declared.  Making the setter
    // protected or private caused an exception because apparently the serializer doesn't check the individual parts
    // and tries to use the setter anyways.  Here are some links related to all this:
    // https://stackoverflow.com/questions/37838640/shouldserialize-vs-specified-conditional-serialization-pattern
    // https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/defining-default-values-with-the-shouldserialize-and-reset-methods?redirectedfrom=MSDN
    // https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer?view=netframework-4.8
    // https://stackoverflow.com/questions/15357589/how-to-tell-xmlserializer-to-serialize-properties-with-defautvalue-always

    // These two links also provide a method where it looks like the XmlSerializer could be modified for the 
    // MessagePropertyAttribute and something like going through each of the properties to check to see
    // if there is a corresponding IsSpecified property could then be used to set up a different serializer type
    // for those types that would return null or something if it shouldn't be serialized.  That would be nice
    // as far as not having to have anything extra in the MessagePropertyAttribute class but is kind of a 
    // lot of extra work and might have other problems.  It has not been tested as of now.
    // https://stackoverflow.com/questions/20084/xml-serialization-and-inherited-types
    // https://www.codeproject.com/Articles/8644/XmlSerializer-and-not-expected-Inherited-Types
    [XmlRoot("MessageSerializerClass", Namespace = "http://www.dorkyengineer.com/MessageSerializerClassInfo/", IsNullable = false)]
    public class ConfigMessageSerializerClass
    {
        [XmlElement(IsNullable = false)]
        public ConfigClassInfo ClassInfo;

        // These functions are for reading and writing from files
        public static void WriteToFile(ConfigMessageSerializerClass messageSerializedClassInfo)
        {
            // The ClassFullName will be something like SomeNamespace.SomethingElse.ClassName
            // We just want the ClassName part
            // LastIndexOf will return -1 if there is no . so by adding 1 we'll get the whole string
            string filename = GetDefaultFileName(messageSerializedClassInfo.ClassInfo.ClassFullName);
            WriteToFile(filename, messageSerializedClassInfo);
        }

        public static void WriteToFile(string filename, ConfigMessageSerializerClass configMessageSerializerClass)
        {
            TextWriter writer = new StreamWriter(filename);

            // The XmlSerializer won't work correctly if you have a List<Something> and there are types
            // that derive from Something, like SomethingElse in the list.  So we go through and find
            // them from the attributes so the XmlSerializer is aware of them.
            // Here's some articles of note:
            // https://stackoverflow.com/questions/11886290/use-the-xmlinclude-or-soapinclude-attribute-to-specify-types-that-are-not-known
            // https://stackoverflow.com/questions/4616505/is-there-a-reason-why-a-base-class-decorated-with-xmlinclude-would-still-throw-a
            // https://stackoverflow.com/questions/2689566/how-to-add-xmlinclude-attribute-dynamically/2689660#2689660
            // https://en.it1352.com/article/4ff854d603034ef3a6364c581b252959.html

            XmlAttributeOverrides attributeOverrides = GetAttributeOverrides(configMessageSerializerClass);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigMessageSerializerClass), attributeOverrides);

            xmlSerializer.Serialize(writer, configMessageSerializerClass);
            writer.Close();
        }

        public static void WriteDefaultToFile(Type type)
        {
            WriteDefaultToFile(GetDefaultFileName(type.FullName), type);
        }

        public static void WriteDefaultToFile(string filename, Type type)
        {
            ConfigMessageSerializerClass configMessageSerializerClass = Serializer.Instance.CreateClassInfoFromType(type);
            WriteToFile(filename, configMessageSerializerClass);
        }

        public void WriteDefaultToFile()
        {
            WriteDefaultToFile(GetType());
        }

        public void WriteDefaultToFile(string filename)
        {
            WriteDefaultToFile(filename, GetType());
        }

        // Do the same with ReadFromFile
        public static ConfigMessageSerializerClass ReadFromFile(string filename)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigMessageSerializerClass), GetAttributeOverrides(GetDefaultExtraTypes()));
            ConfigMessageSerializerClass configMessageSerializerClass = (ConfigMessageSerializerClass)xmlSerializer.Deserialize(fileStream);
            fileStream.Close();
            return configMessageSerializerClass;
        }

        public static ConfigMessageSerializerClass ReadFromFile(Type type)
        {
            return ReadFromFile(GetDefaultFileName(type.FullName));
        }

        protected static string GetDefaultFileName(string className)
        {
            // The ClassFullName will be something like SomeNamespace.SomethingElse.ClassName
            // We just want the ClassName part
            // LastIndexOf will return -1 if there is no . so by adding 1 we'll get the whole string
            return className.Substring(className.LastIndexOf('.') + 1) + ".xml";
        }

        public static XmlAttributeOverrides GetAttributeOverrides(ConfigMessageSerializerClass configMessageSerializerClass)
        {
            var extraTypes = GetExtraTypes(configMessageSerializerClass);
            return GetAttributeOverrides(extraTypes);
        }

        public static XmlAttributeOverrides GetAttributeOverrides(IEnumerable<Type> extraTypes)
        {
            XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();
            XmlAttributes attributes = new XmlAttributes();

            foreach (Type type in extraTypes)
            {
                attributes.XmlElements.Add(new XmlElementAttribute(type.FullName, type));
            }

            attributeOverrides.Add(typeof(ConfigPropertyInfo), "Attributes", attributes);
            return attributeOverrides;
        }

        protected static List<Type> GetExtraTypes(ConfigMessageSerializerClass configMessageSerializerClass)
        {
            var extraTypes = new HashSet<Type>();
            extraTypes.Add(typeof(Attribute));
            foreach (var property in configMessageSerializerClass.ClassInfo.Properties)
            {
                foreach (var attribute in property.Attributes)
                {
                    if (!extraTypes.Contains(attribute.GetType()))
                        extraTypes.Add(attribute.GetType());
                }
            }

            return extraTypes.ToList();
        }

        protected static List<Type> GetDefaultExtraTypes()
        {
            var extraTypes = new List<Type>();
            extraTypes.Add(typeof(Attribute));
            extraTypes.Add(typeof(MessagePropertyAttribute));
            extraTypes.Add(typeof(CalculatedFieldAttribute));
            extraTypes.Add(typeof(CalculatedFieldResultAttribute));
            extraTypes.Add(typeof(CalculatedLengthAttribute));
            extraTypes.Add(typeof(CalculatedLengthResultAttribute));
            extraTypes.Add(typeof(CalculatedAuthenticationAttribute));
            extraTypes.Add(typeof(CalculatedAuthenticationResultAttribute));
            return extraTypes;
        }
    }

    public class ConfigClassInfo
    {
        public ConfigClassInfo()
        {
            Properties = new List<ConfigPropertyInfo>();
            MessageClassAttribute = new MessageClassAttribute();
        }

        [XmlAttribute]
        public string AssemblyName;

        [XmlAttribute]
        public string ClassFullName;

        [XmlIgnore]
        public string AssemblyQualifiedName
        {
            get
            {
                Assembly assembly = Assembly.Load(AssemblyName);
                return ClassFullName + ", " + assembly.FullName;
            }
        }

        [XmlElement]
        public MessageClassAttribute MessageClassAttribute { get; set; }

        [XmlArray("PropertyInfoList")]
        [XmlArrayItem("PropertyInfo")]
        public List<ConfigPropertyInfo> Properties;
    }

    //public class ConfigType
    //{
    //    //protected string _classAssembly;
    //    //protected string _classFullName;

    //    public string ClassAssembly { get; set; }
    //    public string ClassFullName { get; set; }

    //    public ConfigType()
    //    {

    //    }

    //    public ConfigType(Type type)
    //    {
    //        ClassAssembly = GetClassAssembly(type);
    //        ClassFullName = GetClassFullName(type);
    //    }

    //    public string GetClassAssembly(Type type)
    //    {
    //        return type == null ? null : type.Assembly.FullName;
    //    }

    //    public string GetClassFullName(Type type)
    //    {
    //        return type == null ? null : type.FullName;
    //    }

    //    public Type SetClassAssembly(string classAssembly)
    //    {
    //        ClassAssembly = classAssembly;
    //        return GetClassType();
    //    }

    //    public Type SetClassFullName(string classFullName)
    //    {
    //        ClassFullName = classFullName;
    //        return GetClassType();
    //    }

    //    public static implicit operator ConfigType(Type type) => new ConfigType(type);
    //    public static implicit operator Type(ConfigType configType) => configType.GetClassType();

    //    protected Type GetClassType()
    //    {
    //        if (ClassFullName == null)
    //            return null;

    //        if (ClassAssembly == null)
    //            return Type.GetType(ClassFullName);

    //        Assembly assembly = Assembly.Load(ClassAssembly);
    //        return Type.GetType(ClassFullName + ", " + assembly.FullName);
    //    }
    //}

    [XmlRoot("PropertyInfo")]
    public class ConfigPropertyInfo
    {
        public ConfigPropertyInfo()
        {
            Attributes = new List<Attribute>();
        }

        [XmlAttribute]
        public string Name;

        [XmlArray("AttributeList")]
        [XmlArrayItem("Attribute")]
        public List<Attribute> Attributes { get; set; }

        public T GetFirstAttributeOfType<T>() where T : Attribute
        {
            return (T)Attributes.FirstOrDefault(item => item is T);
        }
    }
}
