using System.Collections;
using System.Collections.Generic;

namespace MessageSerializer
{
    public abstract class TypeSerializerBase<T>
    {
        protected MessageSerializedPropertyInfo _propertyInfo;

        protected TypeSerializerBase(MessageSerializedPropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public abstract byte[] Serialize(T value);
        public abstract T Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status);

        public virtual byte[] Serialize<TListType>(TListType list)
            where TListType : IList, IEnumerable<T>
        {
            byte[] returnArray = new byte[0];
            if ((list != null))
            {
                using (IEnumerator<T> itList = list.GetEnumerator())
                {
                    while (itList.MoveNext())
                    {
                        byte[] currentValueArray = Serialize(itList.Current);
                        returnArray = ArrayOps.Combine(returnArray, currentValueArray);
                    }
                }
            }

            return returnArray;
        }

        public virtual TListType DeserializeList<TListType>(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
            where TListType : IList, IEnumerable<T>, new()
        {
            TListType returnList = new TListType();
            for (int endIndexSomeList = (currentArrayIndex + length); (currentArrayIndex < endIndexSomeList); )
            {
                // Note that we don't really have any idea of the length to pass into deserialize for the individual elements
                // so they have to be able to figure it out for themselves
                T element = Deserialize(bytes, ref currentArrayIndex, -1, ref status);
                returnList.Add(element);
            }

            return returnList;
        }

        public virtual string ToString<TListType>(TListType list, int indentLevel, ToStringFormatProperties formatProperties, bool isFirstValue)
            where TListType : IList, IEnumerable<T>, new()
        {
            // Will end up with something like (for List<int> ListPropertyName):
            // ListPropertyName: 
            //     Item 0: 
            //         10
            //     Item 1:
            //         14

            // The indents for Lists can get a little weird because the Fields and ListItems properties can have an effect
            // when the type of the list is a serializable class.  In this scenario there are a few considerations:
            // To make things cleaner it was decided that when the fields are going on a separate line
            // all of the fields should be on the same indent level whereas normally the first field would not be indented
            // and subsequent fields would.  So you would have something like:
            // ListPropertyName:
            //     Item 0:
            //         Field1: 23
            //         Field2: 32
            //     Item 1:
            //         Field1: 44
            //         Field2: 45
            // instead of:
            //     Item 0:
            //         Field1: 23
            //             Field2: 32
            //     Item 1:
            //         Field1: 44
            //             Field2: 45
            // or:
            //     Item 0:
            //     Field1: 23
            //         Field2: 32
            //     Item 1:
            //     Field1: 44
            //         Field2: 45
            //
            // Also, there is a question of what if ListItems.Indent = true and Fields.Indent = true
            // In that scenario you could make an argument that there should then be two indents.
            // However, things will be limited to one indent and you end up with this matrix:
            // ListItems.Indent | Fields.Indent | ToString Indented
            // true             | true          | yes
            // false            | true          | yes
            // true             | false         | yes
            // false            | false         | no
            // By "ToString Indented" being yes it means it will look like this:
            //     Item 0:
            //         Field1: 23
            //         Field2: 32
            //     Item 1:
            //         Field1: 44
            //         Field2: 45
            // "ToString Indented" being no will look like this:
            //     Item 0:
            //     Field1: 23
            //     Field2: 32
            //     Item 1:
            //     Field1: 44
            //     Field2: 45
            // The additional catch here is that the indent for the first field of the object is set by this function
            // and the indent for the subsequent fields is set by the type serializer so we need to make sure that
            // both of those are correct.  So here is a more updated chart:
            // ListItems.Indent | Fields.Indent | ToString Indented | Extra indent here | Pass extra indent to serializer
            // true             | true          | yes               | yes               | no (because serializer will see 2nd field and do the indent itself)
            // false            | true          | yes               | yes               | no (because serializer will see 2nd field and do the indent itself)
            // true             | false         | yes               | yes               | yes (because serializer will not indent the fields)
            // false            | false         | no                | no                | no

            indentLevel = formatProperties.Fields.GetNewIndentLevel(indentLevel, isFirstValue);

            // ListPropertyName: 
            string returnValue = string.Format("{0}{1}{2}{3}{4}",
                formatProperties.Fields.GetSeparator(isFirstValue),
                formatProperties.Fields.GetIndentString(indentLevel, isFirstValue),
                formatProperties.Fields.Prefix,
                _propertyInfo.PropertyInfo.Name,
                formatProperties.Fields.NameValueSeparator);

            int index = 0;
            using (IEnumerator<T> itList = list.GetEnumerator())
            {
                for (; itList.MoveNext(); ++index)
                {
                    int currentIndentLevel = formatProperties.ListItemHeaders.GetNewIndentLevel(indentLevel, false);

                    if (formatProperties.NumberListItems)
                    {
                        // Index 0: 
                        returnValue += string.Format("{0}{1}{2}{3}{4}{5}",
                            formatProperties.ListItemHeaders.GetSeparator(index == 0),
                            formatProperties.ListItemHeaders.GetIndentString(currentIndentLevel, false),
                            formatProperties.ListItemHeaders.Prefix,
                            formatProperties.ListItemName,
                            formatProperties.UseOneBasedListIndex ? index + 1 : index,
                            formatProperties.ListItemHeaders.NameValueSeparator);
                    }

                    //     Value (see big pile of notes above about the indent level
                    int valueInitialIndentLevel = (formatProperties.ListItems.Indent || formatProperties.Fields.Indent) ? currentIndentLevel + 1 : currentIndentLevel;
                    int toStringIndentLevel = (formatProperties.ListItems.Indent && !formatProperties.Fields.Indent) ? currentIndentLevel + 1 : currentIndentLevel;
                    returnValue += string.Format("{0}{1}{2}{3}{4}",
                        formatProperties.ListItems.GetSeparator(index == 0),
                        formatProperties.ListItems.GetIndentString(valueInitialIndentLevel, false),
                        formatProperties.ListItems.Prefix,
                        GetToStringValue(itList.Current, toStringIndentLevel, formatProperties, true),
                        formatProperties.ListItems.Suffix);

                    if (formatProperties.NumberListItems)
                        returnValue += formatProperties.ListItemHeaders.Suffix;
                }
            }

            returnValue += $"{formatProperties.Fields.Suffix}";
            return returnValue;
        }

        public virtual string ToString(T value, int indentLevel, ToStringFormatProperties formatProperties, bool isFirstValue)
        {
            indentLevel = formatProperties.Fields.GetNewIndentLevel(indentLevel, isFirstValue);

            return string.Format("{0}{1}{2}{3}{4}{5}{6}",
                formatProperties.Fields.GetSeparator(isFirstValue),
                formatProperties.Fields.GetIndentString(indentLevel, isFirstValue),
                formatProperties.Fields.Prefix,
                _propertyInfo.PropertyInfo.Name,
                formatProperties.Fields.NameValueSeparator,
                GetToStringValue(value, indentLevel, formatProperties, false),
                formatProperties.Fields.Suffix);
        }

        protected virtual string GetToStringValue(T value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)
        {
            return value.ToString();
        }

        protected virtual int GetLength()
        {
            return _propertyInfo.MessagePropertyAttribute.Length;
        }
    }
}
