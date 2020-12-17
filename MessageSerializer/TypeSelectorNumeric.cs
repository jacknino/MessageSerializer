using System;

namespace MessageSerializer
{
    /// <summary>
    /// Type selector for numeric types (short, int, uint, ulong, etc.)
    /// </summary>
    public class TypeSelectorNumeric : ITypeSelector
    {
        public virtual Type CheckType(MessageSerializedPropertyInfo propertyInfo)
        {
            if (NumericFunctions.IsIntegerType(propertyInfo.ElementType)
                && NumericFunctions.IsPowerOfTwo((ulong)propertyInfo.MessagePropertyAttribute.Length))
            {
                return typeof(TypeSerializerNumeric<>).MakeGenericType(new[] { propertyInfo.ElementType });
            }

            return null;
        }
    }
}
