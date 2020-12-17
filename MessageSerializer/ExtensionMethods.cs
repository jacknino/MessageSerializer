using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageSerializer
{
    public static class ExtensionMethods
    {
        private static readonly Dictionary<Type, string> ShorthandMap = new Dictionary<Type, string>
        {
            { typeof(Boolean), "bool" },
            { typeof(Byte), "byte" },
            { typeof(Char), "char" },
            { typeof(Decimal), "decimal" },
            { typeof(Double), "double" },
            { typeof(Single), "float" },
            { typeof(Int32), "int" },
            { typeof(Int64), "long" },
            { typeof(SByte), "sbyte" },
            { typeof(Int16), "short" },
            { typeof(String), "string" },
            { typeof(UInt32), "uint" },
            { typeof(UInt64), "ulong" },
            { typeof(UInt16), "ushort" },
        };

        public static string PrintableType(this Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return $"{PrintableType(Nullable.GetUnderlyingType(type))}?";
                }

                return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(PrintableType).ToArray())}>";
            }

            if (type.IsArray)
            {
                return $"{PrintableType(type.GetElementType())}[]";
            }

            return ShorthandMap.ContainsKey(type) ? ShorthandMap[type] : type.Name;
        }

        public static string GetIndent(this int indentLevel, string indentString = "    ")
        {
            return new StringBuilder().Insert(0, indentString, indentLevel).ToString();
        }

        public static StringBuilder Join(this StringBuilder stringBuilder, string stringToAppend, string joinString = ", ")
        {
            if (stringBuilder.Length != 0)
                stringBuilder.Append(joinString);

            return stringBuilder.Append(stringToAppend);
        }

        public static StringBuilder ConditionalJoin(this StringBuilder stringBuilder, string stringToAppend, string joinString = ", ")
        {
            if (string.IsNullOrEmpty(stringToAppend))
                return stringBuilder;

            return stringBuilder.Join(stringToAppend, joinString);
        }

        public static string GetNameValuePair(this string name, bool isSpecified, object value)
        {
            if (!isSpecified)
                return "";

            return $"{name}: {value}";
        }
    }
}
