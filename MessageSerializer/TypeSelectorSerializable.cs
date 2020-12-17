using System;

namespace MessageSerializer
{
    /// <summary>
    /// Checks to see if a class is serializable and if so returns the TypeSerializerSerialiableClass
    /// </summary>
    public class TypeSelectorSerializable : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (propertyInfo.ElementIsMessageSerializableObject)
            {
                return typeof(TypeSerializerSerializableClass<>).MakeGenericType(new[] { propertyInfo.ElementType });
            }

            return null;
        }
    }
}
