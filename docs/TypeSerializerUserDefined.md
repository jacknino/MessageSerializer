---
layout: defaultWithNavigation
title: User Defined TypeSerializers
---
## {{ page.title }}

To define your own TypeSerializers you can either inherit from `TypeSerializerBase` or from a pre-existing TypeSerializer.
This gives you a large amount of flexibility for when you have a protocol that has a non-standard way of outputting some values.

If you do create your own TypeSerializer, in order to use it you have two options:
* Set the `TypeSerializerClass` property of the {{ site.data.linkVariables["MessagePropertyAttribute"] }} of the property
you want to use it on to the `Type` of your TypeSerializer
* Create your own {{ site.data.linkVariables["TypeSelector"] }} that checks for some condition appropriate to your property and then returns your TypeSelector.

As a quick example, let's take a field that is numeric but should always serialize as 3-bytes.

```csharp
public class TypeSerializerThreeByteNumeric : TypeSerializerBase<int>
{
    public TypeSerializerThreeByteNumeric(MessageSerializedPropertyInfo propertyInfo)
        : base(propertyInfo)
    {
    }

    public override byte[] Serialize(int value)
    {
        // We want to cut off the most significant byte so we get the array as little endian
        // then cut off the last byte (the MSB) and then determine if we need to reverse the bytes
        byte[] returnArray = ArrayOps.GetBytesFromNumeric(value, Endiannesses.Little);
        if (returnArray[3] != 0)
            throw new Exception($"For {_propertyInfo.PropertyInfo.Name}, {value} can not be serialized into a 3-byte value");

        return ArrayOps.GetSubArray(returnArray, 0, 3, ArrayOps.EndiannessRequiresReversal(Endiannesses.Little, _propertyInfo.MessagePropertyAttribute.Endianness));
    }

    public override int Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status)
    {
        // GetNumeric expects there to be the correct number of bytes for the type it is converting to
        // So we need to first get the 3 bytes we want and get them in little endian order
        // This is so when we resize to 4 bytes the 0x00 for the MSB will be correctly put at the end
        // Then when we call GetNumeric we make sure to indicate that the bytes are currently in little endian order
        byte[] workingArray = ArrayOps.GetSubArray(bytes, currentArrayIndex, 3, ArrayOps.EndiannessRequiresReversal(_propertyInfo.MessagePropertyAttribute.Endianness, Endiannesses.Little));
        Array.Resize(ref workingArray, 4);
        int value = ArrayOps.GetNumeric<int>(workingArray, Endiannesses.Little);
        currentArrayIndex += 3;
        return value;
    }
}
```

Some things to note:
* This TypeSerializer supports serializing and deserializing the value into whatever Endianness is required (`System`, `Little` or `Big`)
* The desired Endianness comes from the `_propertyInfo.MessagePropertyAttribute.Endianness`
* When deserializing, make sure to increment currentArrayIndex the number of bytes you "took" from the `bytes` array so the TypeSerializer for the next property in the message will start retrieving bytes from the correct place.

