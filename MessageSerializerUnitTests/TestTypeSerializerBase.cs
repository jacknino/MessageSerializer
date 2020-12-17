using System;
using System.Collections;
using System.Collections.Generic;
using MessageSerializer;
using NUnit.Framework;

namespace MessageSerializerUnitTests
{
    public abstract class TestTypeSerializerBase
    {
        protected MessageSerializedPropertyInfo GetPropertyInfo(MessageSerializedClassInfo classInfo, string propertyName)
        {
            foreach (MessageSerializedPropertyInfo propertyInfo in classInfo.Properties)
            {
                if (propertyInfo.PropertyInfo.Name == propertyName)
                    return propertyInfo;
            }

            throw new Exception($"Couldn't find property {propertyName} in {classInfo.ClassType.FullName}");
        }

        protected virtual void TestField<TTypeSerializer, TValueType>(TTypeSerializer typeSerializer, MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray) 
            where TTypeSerializer : TypeSerializerBase<TValueType>
        {
            byte[] serializedArray = typeSerializer.Serialize(valueToUse);
            Assert.That(serializedArray, Is.EqualTo(expectedArray), "Serialize");

            DeserializeStatus status = new DeserializeStatus();
            int currentArrayIndex = 0;
            TValueType deserializedValue = typeSerializer.Deserialize(expectedArray, ref currentArrayIndex, expectedArray.Length, ref status);
            VerifyDeserialized(deserializedValue, valueToUse, propertyInfo, expectedArray);

            CheckToString(typeSerializer, propertyInfo, valueToUse, expectedArray);
        }

        protected virtual void CheckToString<TTypeSerializer, TValueType>(TTypeSerializer typeSerializer, MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray) 
            where TTypeSerializer : TypeSerializerBase<TValueType>
        {
            string expectedToStringResult = $"{propertyInfo.PropertyInfo.Name}: " + GetToStringValue(propertyInfo, valueToUse, expectedArray);
            string toStringResult = typeSerializer.ToString(valueToUse, 0, ToStringFormatProperties.Default, true);
            Assert.That(toStringResult, Is.EqualTo(expectedToStringResult), "ToString");
        }

        protected virtual string GetToStringValue<TValueType>(MessageSerializedPropertyInfo propertyInfo, TValueType valueToUse, byte[] expectedArray)
        {
            string expectedFormatString = GetExpectedToStringValueFormat(propertyInfo);
            return string.Format(expectedFormatString, valueToUse);
        }

        protected virtual void TestListField<TTypeSerializer, TListType, TValueType>(TTypeSerializer typeSerializer, MessageSerializedPropertyInfo propertyInfo, TListType listToUse, byte[] expectedArray)
            where TTypeSerializer : TypeSerializerBase<TValueType>
            where TListType : IEnumerable<TValueType>, IList, new()
        {
            byte[] serializedArray = typeSerializer.Serialize(listToUse);
            Assert.That(serializedArray, Is.EqualTo(expectedArray), "Serialize");

            DeserializeStatus status = new DeserializeStatus();
            int currentArrayIndex = 0;
            TListType deserializedList = typeSerializer.DeserializeList<TListType>(expectedArray, ref currentArrayIndex, expectedArray.Length, ref status);
            VerifyDeserializedList<TListType, TValueType>(deserializedList, listToUse, propertyInfo, expectedArray);

            ToStringFormatProperties formatProperties = ToStringFormatProperties.Default;
            string expectedToStringResult = GetExpectedListToString<TListType, TValueType>(propertyInfo, listToUse, formatProperties);
            string toStringResult = typeSerializer.ToString(listToUse, 0, formatProperties, true);
            Assert.That(toStringResult, Is.EqualTo(expectedToStringResult), "ToString");
        }

        protected virtual void VerifyDeserialized<TValueType>(TValueType deserializedValue, TValueType expectedValue, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
        {
            Assert.That(deserializedValue, Is.EqualTo(expectedValue), "Deserialize");
        }

        protected virtual void VerifyDeserializedList<TListType, TValueType>(TListType deserializedList, TListType expectedList, MessageSerializedPropertyInfo propertyInfo, byte[] valueArray)
            where TListType : IEnumerable<TValueType>, IList, new()
        {
            Assert.That(deserializedList.Count, Is.EqualTo(expectedList.Count), "ListCount");
            for (int index = 0; index < deserializedList.Count; ++index)
            {
                VerifyDeserialized(deserializedList[index], expectedList[index], propertyInfo, valueArray);
            }
        }

        protected virtual string GetExpectedToStringValueFormat(MessageSerializedPropertyInfo propertyInfo)
        {
            return "{0}";
        }

        protected virtual string GetExpectedListToString<TListType, TValueType>(MessageSerializedPropertyInfo propertyInfo, TListType listToUse, ToStringFormatProperties formatProperties)
            where TListType : IEnumerable<TValueType>, IList, new()
        {
            string expectedFormatString = GetExpectedToStringValueFormat(propertyInfo);

            string expectedToStringResult = string.Format("{0}{1}{2}", formatProperties.Fields.Prefix, propertyInfo.PropertyInfo.Name, formatProperties.Fields.NameValueSeparator);
            for (int index = 0; index < listToUse.Count; ++index)
            {
                expectedToStringResult += formatProperties.ListItems.GetSeparator(index == 0);

                int currentIndentLevel = 0;
                if (formatProperties.NumberListItems)
                {
                    expectedToStringResult += $"{formatProperties.ListItemHeaders.GetSeparator(index == 0)}{formatProperties.ListItemHeaders.GetIndentString(currentIndentLevel + 1, false)}{formatProperties.ListItemName}{index}{formatProperties.ListItemHeaders.NameValueSeparator}";
                }

                expectedToStringResult += formatProperties.ListItems.GetIndentString(currentIndentLevel + 2, false);
                expectedToStringResult += formatProperties.ListItems.Prefix;
                expectedToStringResult += string.Format(expectedFormatString, listToUse[index]);
                expectedToStringResult += formatProperties.ListItems.Suffix;
            }

            expectedToStringResult += formatProperties.Fields.Suffix;

            return expectedToStringResult;
        }
    }
}
