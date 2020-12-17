using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    [TestFixture]
    public class TestClassInfoXml
    {
        public class SimpleXmlTestClass : IMessageSerializable
        {
            public byte MessageType { get; set; }
            public uint Value1 { get; set; }
            public byte Length { get; set; }
            public string ValueString { get; set; }
        }

        // Some example code: https://docs.microsoft.com/en-us/dotnet/standard/serialization/examples-of-xml-serialization
        ConfigMessageSerializerClass CreateClassInfo()
        {
            ConfigMessageSerializerClass messageSerializerClassInfo = new ConfigMessageSerializerClass();
            ConfigClassInfo classInfo = new ConfigClassInfo();
            classInfo.AssemblyName = Assembly.GetExecutingAssembly().FullName;
            classInfo.ClassFullName = typeof(SimpleXmlTestClass).FullName;

            ConfigPropertyInfo propertyInfo1 = new ConfigPropertyInfo();
            propertyInfo1.Name = "Property1";
            propertyInfo1.Attributes.Add(new MessagePropertyAttribute() { PrepadCharacter = 'T' });
            propertyInfo1.Attributes.Add(new CalculatedLengthResultAttribute());
            //propertyInfo1._messagePropertyAttribute.MessageLengthType = MessageLengthTypes.RestOfMessage;
            //propertyInfo1._messagePropertyAttribute.PrepadCharacter = 'T';
            //propertyInfo1.AuthenticationClass = typeof(int);
            classInfo.Properties.Add(propertyInfo1);

            ConfigPropertyInfo propertyInfo2 = new ConfigPropertyInfo();
            propertyInfo2.Name = "Property2";
            classInfo.Properties.Add(propertyInfo2);

            ConfigPropertyInfo propertyInfo3 = new ConfigPropertyInfo();
            propertyInfo3.Name = "Property3";
            var property3MessagePropertyAttribute = new MessagePropertyAttribute();
            property3MessagePropertyAttribute.IsBcd = true;
            //property3MessagePropertyAttribute.IsAuthenticationField = false;
            //property3MessagePropertyAttribute.ExcludeFromAuthentication = true;
            property3MessagePropertyAttribute.Length = 20;
            property3MessagePropertyAttribute.Format = "YYYYMMDDHHMMSS";
            property3MessagePropertyAttribute.BlobType = BlobTypes.None;
            property3MessagePropertyAttribute.VariableLength = true;
            property3MessagePropertyAttribute.MinLength = 1;
            property3MessagePropertyAttribute.MaxLength = 18;
            property3MessagePropertyAttribute.MinimizeVariableLength = true;
            //property3MessagePropertyAttribute.ExcludeFromLength = false;
            property3MessagePropertyAttribute.PrepadCharacter = ' ';
            //property3MessagePropertyAttribute.MessageLengthType = MessageLengthTypes.RestOfMessage;
            //property3MessagePropertyAttribute.AuthenticationClass = typeof(AuthenticationCrc32);
            propertyInfo3.Attributes.Add(property3MessagePropertyAttribute);
            propertyInfo3.Attributes.Add(new CalculatedLengthResultAttribute() { Exclude = false });
            propertyInfo3.Attributes.Add(new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationCrc32)));
            classInfo.Properties.Add(propertyInfo3);

            messageSerializerClassInfo.ClassInfo = classInfo;

            return messageSerializerClassInfo;
        }

        void VerifyMessageSerializedClassAttribute(MessageClassAttribute classInfoToVerify, MessageClassAttribute classInfoExpected, string infoPrefix)
        {
            Assert.That(classInfoToVerify.DefaultExcludeProperty, Is.EqualTo(classInfoExpected.DefaultExcludeProperty), infoPrefix + "::DefaultExcludeProperty");
            Assert.That(classInfoToVerify.PutInheritedPropertiesLast, Is.EqualTo(classInfoExpected.PutInheritedPropertiesLast), infoPrefix + "::PutInheritedPropertiesLast");
        }

        void VerifyMessageSerializedPropertyAttribute(MessagePropertyAttribute propertyInfoToVerify, MessagePropertyAttribute propertyInfoExpected, string infoPrefix)
        {
            Assert.That(propertyInfoToVerify.Exclude, Is.EqualTo(propertyInfoExpected.Exclude), infoPrefix + "::Exclude");
            //Assert.That(propertyInfoToVerify.ExcludeFromLength, Is.EqualTo(propertyInfoExpected.ExcludeFromLength), infoPrefix + "::ExcludeFromLength");
            Assert.That(propertyInfoToVerify.Prepad, Is.EqualTo(propertyInfoExpected.Prepad), infoPrefix + "::Prepad");
            Assert.That(propertyInfoToVerify.PrepadCharacter, Is.EqualTo(propertyInfoExpected.PrepadCharacter), infoPrefix + "::PrepadCharacter");
            //Assert.That(propertyInfoToVerify.MessageLengthType, Is.EqualTo(propertyInfoExpected.MessageLengthType), infoPrefix + "::MessageLengthType");
            Assert.That(propertyInfoToVerify.BlobType, Is.EqualTo(propertyInfoExpected.BlobType), infoPrefix + "::BlobType");
            Assert.That(propertyInfoToVerify.AssociatedBlobProperty, Is.EqualTo(propertyInfoExpected.AssociatedBlobProperty), infoPrefix + "::AssociatedBlobProperty");
            Assert.That(propertyInfoToVerify.IsBcd, Is.EqualTo(propertyInfoExpected.IsBcd), infoPrefix + "::IsBcd");
            //Assert.That(propertyInfoToVerify.IsAuthenticationField, Is.EqualTo(propertyInfoExpected.IsAuthenticationField), infoPrefix + "::IsAuthenticationField");
            //Assert.That(propertyInfoToVerify.AuthenticationClass, Is.EqualTo(propertyInfoExpected.AuthenticationClass), infoPrefix + "::AuthenticationClass");
            //Assert.That(propertyInfoToVerify.ExcludeFromAuthentication, Is.EqualTo(propertyInfoExpected.ExcludeFromAuthentication), infoPrefix + "::ExcludeFromAuthentication");
            Assert.That(propertyInfoToVerify.Length, Is.EqualTo(propertyInfoExpected.Length), infoPrefix + "::Length");
            Assert.That(propertyInfoToVerify.VariableLength, Is.EqualTo(propertyInfoExpected.VariableLength), infoPrefix + "::VariableLength");
            Assert.That(propertyInfoToVerify.MinLength, Is.EqualTo(propertyInfoExpected.MinLength), infoPrefix + "::MinLength");
            Assert.That(propertyInfoToVerify.MaxLength, Is.EqualTo(propertyInfoExpected.MaxLength), infoPrefix + "::MaxLength");
            Assert.That(propertyInfoToVerify.MinimizeVariableLength, Is.EqualTo(propertyInfoExpected.MinimizeVariableLength), infoPrefix + "::MinimizeVariableLength");
            Assert.That(propertyInfoToVerify.Format, Is.EqualTo(propertyInfoExpected.Format), infoPrefix + "::Format");
        }

        void VerifyCalculatedLengthResultAttribute(CalculatedLengthResultAttribute calculatedFieldResultToVerify, CalculatedLengthResultAttribute calculatedFieldResultExpected, string infoPrefix)
        {
            Assert.That(calculatedFieldResultToVerify.Exclude, Is.EqualTo(calculatedFieldResultExpected.Exclude), infoPrefix + "::Exclude");
            Assert.That(calculatedFieldResultToVerify.Start, Is.EqualTo(calculatedFieldResultExpected.Start), infoPrefix + "::Start");
        }

        void VerifyCalculatedAuthenticationResultAttribute(CalculatedAuthenticationResultAttribute calculatedFieldResultToVerify, CalculatedAuthenticationResultAttribute calculatedFieldResultExpected, string infoPrefix)
        {
            Assert.That(calculatedFieldResultToVerify.Calculator, Is.EqualTo(calculatedFieldResultExpected.Calculator), infoPrefix + "::Calculator");
            //Assert.That(calculatedFieldResultToVerify.Exclude, Is.EqualTo(calculatedFieldResultExpected.Exclude), infoPrefix + "::Exclude");
            //Assert.That(calculatedFieldResultToVerify.Start, Is.EqualTo(calculatedFieldResultExpected.Start), infoPrefix + "::Start");
        }
        void VerifyClassInfo(ConfigMessageSerializerClass classInfoToVerify, ConfigMessageSerializerClass classInfoExpected)
        {
            Assert.That(classInfoToVerify.ClassInfo, Is.Not.Null);
            Assert.That(classInfoToVerify.ClassInfo.AssemblyQualifiedName, Is.EqualTo(classInfoExpected.ClassInfo.AssemblyQualifiedName));
            VerifyMessageSerializedClassAttribute(classInfoToVerify.ClassInfo.MessageClassAttribute, classInfoExpected.ClassInfo.MessageClassAttribute, classInfoToVerify.ClassInfo.AssemblyQualifiedName);

            Assert.That(classInfoToVerify.ClassInfo.Properties, Is.Not.Null);
            Assert.That(classInfoToVerify.ClassInfo.Properties.Count, Is.EqualTo(classInfoExpected.ClassInfo.Properties.Count));

            for (int index = 0; index < classInfoToVerify.ClassInfo.Properties.Count; ++index)
            {
                ConfigPropertyInfo propertyInfoToVerify = classInfoToVerify.ClassInfo.Properties[index];
                ConfigPropertyInfo propertyInfoExpected = classInfoExpected.ClassInfo.Properties[index];

                Assert.That(propertyInfoToVerify.Name, Is.EqualTo(propertyInfoExpected.Name));
                //VerifyMessageSerializedPropertyAttribute(propertyInfoToVerify._messagePropertyAttribute, propertyInfoExpected._messagePropertyAttribute, string.Format("{0}::Index{1}", classInfoToVerify.ClassInfo, index));
                VerifyMessageSerializedPropertyAttribute(propertyInfoToVerify.GetFirstAttributeOfType<MessagePropertyAttribute>() ?? new MessagePropertyAttribute(), propertyInfoExpected.GetFirstAttributeOfType<MessagePropertyAttribute>() ?? new MessagePropertyAttribute(), string.Format("{0}::Index{1}", classInfoToVerify.ClassInfo, index));
                VerifyCalculatedLengthResultAttribute(propertyInfoToVerify.GetFirstAttributeOfType<CalculatedLengthResultAttribute>() ?? new CalculatedLengthResultAttribute(), propertyInfoExpected.GetFirstAttributeOfType<CalculatedLengthResultAttribute>() ?? new CalculatedLengthResultAttribute(), string.Format("{0}::Index{1}", classInfoToVerify.ClassInfo, index));
                VerifyCalculatedAuthenticationResultAttribute(propertyInfoToVerify.GetFirstAttributeOfType<CalculatedAuthenticationResultAttribute>() ?? new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationCrc16)), propertyInfoExpected.GetFirstAttributeOfType<CalculatedAuthenticationResultAttribute>() ?? new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationCrc16)), string.Format("{0}::Index{1}", classInfoToVerify.ClassInfo, index));
            }
        }

        [Test]
        public void DoSimpleTest()
        {
            string filename = "SimpleTestClass.xml";
            ConfigMessageSerializerClass testMessageSerializerClassInfo = CreateClassInfo();
            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigMessageSerializerClass), new Type[] { typeof(MessagePropertyAttribute), typeof(CalculatedLengthResultAttribute)});

            XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();
            XmlAttributes attributes = new XmlAttributes();
            attributes.XmlElements.Add(new XmlElementAttribute(typeof(MessagePropertyAttribute).FullName, typeof(MessagePropertyAttribute)));
            attributes.XmlElements.Add(new XmlElementAttribute(typeof(CalculatedLengthResultAttribute).FullName, typeof(CalculatedLengthResultAttribute)));
            attributes.XmlElements.Add(new XmlElementAttribute(typeof(CalculatedAuthenticationResultAttribute).FullName, typeof(CalculatedAuthenticationResultAttribute)));
            attributeOverrides.Add(typeof(ConfigPropertyInfo), "Attributes", attributes);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigMessageSerializerClass), attributeOverrides);

            TextWriter writer = new StreamWriter(filename);
            xmlSerializer.Serialize(writer, testMessageSerializerClassInfo);
            writer.Close();

            // If the XML document has been altered with unknown
            // nodes or attributes, handles them with the
            // UnknownNode and UnknownAttribute events.
            //serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
            //serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);

            FileStream fileStream = new FileStream(filename, FileMode.Open);
            ConfigMessageSerializerClass readMessageSerializerClassInfo = (ConfigMessageSerializerClass)xmlSerializer.Deserialize(fileStream);

            VerifyClassInfo(readMessageSerializerClassInfo, testMessageSerializerClassInfo);
        }

        [Test]
        public void TestSimpleXmlTestClass()
        {
            string filename = "SimpleXmlTestClass.xml";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigMessageSerializerClass));
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            ConfigMessageSerializerClass readMessageSerializerClassInfo = (ConfigMessageSerializerClass)xmlSerializer.Deserialize(fileStream);
            List<ConfigMessageSerializerClass> configMessageSerializerClasses = new List<ConfigMessageSerializerClass>();
            configMessageSerializerClasses.Add(readMessageSerializerClassInfo);

            Serializer.Instance.LoadSerializableClassesFromSettings(configMessageSerializerClasses);

            SimpleXmlTestClass simpleXmlTestClass = new SimpleXmlTestClass();
            simpleXmlTestClass.MessageType = 0x03;
            simpleXmlTestClass.Value1 = 0x12345678;
            simpleXmlTestClass.ValueString = "ValueString";

            byte[] serializedBytes = Serializer.Instance.Serialize(simpleXmlTestClass);

            SimpleXmlTestClass deserializedSimpleXmlTestClass = Serializer.Instance.Deserialize<SimpleXmlTestClass>(serializedBytes);
            Assert.That(simpleXmlTestClass.MessageType, Is.EqualTo(deserializedSimpleXmlTestClass.MessageType));
            Assert.That(simpleXmlTestClass.Value1, Is.EqualTo(deserializedSimpleXmlTestClass.Value1));
            Assert.That(simpleXmlTestClass.Length, Is.EqualTo(deserializedSimpleXmlTestClass.Length));
            Assert.That(simpleXmlTestClass.ValueString, Is.EqualTo(deserializedSimpleXmlTestClass.ValueString));
        }

        //private ConfigMessageSerializerClass CreateClassInfoFromType<T>()
        //{
        //    MessageSerializedClassInfo MessageSerializedClassInfo = Serializer.Instance.GetClassInfo(typeof(T), true);

        //    ConfigMessageSerializerClass configMessageSerializerClass = new ConfigMessageSerializerClass();
        //    ConfigClassInfo configClassInfo = new ConfigClassInfo();
        //    configClassInfo.AssemblyName = MessageSerializedClassInfo.ClassType.AssemblyQualifiedName;
        //    configClassInfo.ClassFullName = MessageSerializedClassInfo.ClassType.FullName;
        //    configClassInfo.MessageClassAttribute = MessageSerializedClassInfo.MessageClassAttribute;
        //    configMessageSerializerClass.ClassInfo = configClassInfo;

        //    foreach (MessageSerializedPropertyInfo MessageSerializedPropertyInfo in MessageSerializedClassInfo.Properties)
        //    {
        //        ConfigPropertyInfo configPropertyInfo = new ConfigPropertyInfo();
        //        configPropertyInfo.Name = MessageSerializedPropertyInfo.PropertyInfo.Name;
        //        configPropertyInfo.MessagePropertyAttribute = MessageSerializedPropertyInfo.MessagePropertyAttribute;
        //        configClassInfo.Properties.Add(configPropertyInfo);
        //    }

        //    return configMessageSerializerClass;
        //}
    }
}
