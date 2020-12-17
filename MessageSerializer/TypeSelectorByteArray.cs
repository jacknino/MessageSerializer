using System;

namespace MessageSerializer
{
    public class TypeSelectorByteArray : ITypeSelector
    {
        public Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyInfo.PropertyType == typeof(byte[]))
            {
                return typeof(TypeSerializerByteArray);
            }

            return null;
        }
    }
}
