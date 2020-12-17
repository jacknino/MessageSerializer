using System;

namespace MessageSerializer
{
    /// <summary>
    /// Used to check for the TypeSerializerClass attribute has been set on the MessageSerializedProperty attribute
    /// If it has been set then that Type is checked to make sure it is of type TypeSerializerBase and that type is returned
    /// </summary>
    public class TypeSelectorSpecifiedClass : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            Type specifiedClass = propertyInfo.MessagePropertyAttribute.TypeSerializerClass;
            if (specifiedClass != null)
            {
                // Double check that TypeSerializerClass actually inherits from TypeSerializerBase
                for (Type currentType = propertyInfo.MessagePropertyAttribute.TypeSerializerClass; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
                {
                    Type typeToCheck = currentType.IsGenericType ? currentType.GetGenericTypeDefinition() : currentType;
                    if (typeof(TypeSerializerBase<>) == typeToCheck)
                    {
                        return specifiedClass;
                    }
                }

                throw new Exception($"{specifiedClass.FullName} was specified as the TypeSerializerClass to use for property {propertyInfo.PropertyInfo.Name} of type {propertyInfo.ElementType.FullName} but it does not inherit from TypeSerializerBase<>");
            }

            return null;
        }
    }
}
