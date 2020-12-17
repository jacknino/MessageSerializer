using System;

namespace MessageSerializer
{
    // TypeSelector for strings
    public class TypeSelectorString : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.ElementType.FullName == typeof(string).FullName)
            {
                return typeof(TypeSerializerString);
            }

            return null;
        }
    }
}
