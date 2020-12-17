using System;

namespace MessageSerializer
{
    /// <summary>
    /// TypeSelector for enums
    /// </summary>
    public class TypeSelectorEnum : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.ElementType.IsEnum)
            {
                return typeof(TypeSerializerEnum<>).MakeGenericType(new[] { propertyInfo.ElementType });
            }

            return null;
        }
    }
}
