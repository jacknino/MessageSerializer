---
layout: defaultWithNavigation
title: User Defined TypeSelectors
---
## {{ page.title }}

Creating your own `TypeSelector` is a relatively easy process.  Essentially all you need to do is:

1. Create your your class that inherits from `ITypeSelector`
1. Implement the `CheckType` function
1. Add an instance of your class to the `TypeSelectors` of the {{ site.data.linkVariables["SerializationDefaults"] }} that you
are going to use when loading your classes that should take advantage of your TypeSelector.

### Example

For our example we are going to build on the <makeLink>TypeSerializerUserDefined</makeLink> example.  In that example, we made
a `TypeSerializer` that will serialize an `int` into a three byte array and then deserialize three bytes into an `int`.

For our `TypeSelector` we will assume that any property that has a type of `int` and whose name starts with `ThreeByte` should
use the `TypeSerializerThreeByteNumeric` that we defined in the other example.

Our TypeSelector can be implemented as shown below.  Note that when your TypeSelector does not know how to handle the property
it should return null so that any further TypeSelectors can be checked.

```csharp
public class TypeSelectorThreeByteNumeric : ITypeSelector
{
    public Type CheckType(MessageSerializedPropertyInfo propertyInfo)
    {
        if (propertyInfo.PropertyInfo.PropertyType == typeof(int)
            && propertyInfo.PropertyInfo.Name.StartsWith("ThreeByte"))
        {
            return typeof(TypeSerializerThreeByteNumeric);
        }

        return null;
    }
}
```

Make sure you are returning the `TypeSerializer` and not the `TypeSelector`.

Now you need to add your TypeSelector to the `TypeSelectors` list of the {{ site.data.linkVariables["SerializationDefaults"] }}
and then use those SerializationDefaults when loading whatever class that you want your TypeSelector to apply to.

Note that in the code below our new TypeSelector is put into the list first.  The reason for this is that it has to be put before
<makeLink>TypeSelectorNumeric</makeLink> as TypeSelectorNumeric will match with an `int` property and if our TypeSelector was
at the end of the list its `CheckType` would never be called because the property was already matched.

```csharp
var serializationDefaults = new SerializationDefaults();
serializationDefaults.TypeSelectors.Insert(0, new TypeSelectorThreeByteNumeric());
_classInfo = Serializer.Instance.GetClassInfo(typeof(TestClass), serializationDefaults);
```
