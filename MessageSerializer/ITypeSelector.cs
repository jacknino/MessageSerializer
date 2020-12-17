using System;

namespace MessageSerializer
{
    public interface ITypeSelector
    {
        Type CheckType(MessageSerializedPropertyInfo propertyInfo);
    }
}
