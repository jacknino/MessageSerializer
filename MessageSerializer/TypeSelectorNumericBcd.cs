using System;

namespace MessageSerializer
{
    /// <summary>
    /// Used to select types that should be numeric BCD
    /// </summary>
    public class TypeSelectorNumericBcd : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.MessagePropertyAttribute.IsBcd
                && propertyInfo.ElementType.FullName != typeof(DateTime).FullName)
            {
                return typeof(TypeSerializerBcd<>).MakeGenericType(new[] { propertyInfo.ElementType });
            }

            return null;
        }
    }
}
