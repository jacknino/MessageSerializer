using System;

namespace MessageSerializer
{
    public class TypeSelectorDateTime : ITypeSelector
    {
        public Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.ElementType.FullName == typeof(DateTime).FullName
                && propertyInfo.MessagePropertyAttribute.IsBcd)
            {
                return typeof(TypeSerializerDateTime);
            }

            return null;
        }
    }
}
