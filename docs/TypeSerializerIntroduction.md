---
layout: defaultWithNavigation
title: TypeSerializer Introduction
---
## Introduction to TypeSerializers

TypeSerializers are the classes that do the actual work of serializing and deserializing the values in a class as well as providing a ToString method
so that the contents of a class can be displayed for debugging purposes.  All TypeSerializers inherit from `TypeSerializerBase<T>`. `T` is the `ElementType` of
the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}.  The TypeSerializers that are used for a class are selected using the 
<makeLink link="TypeSelectorIntroduction">TypeSelectors</makeLink>

All TypeSerializers must implement the following two functions:

```charp
byte[] Serialize(T value);
T Deserialize(byte[] bytes, ref int currentArrayIndex, int length, ref DeserializeStatus status);
```

Notes on the TypeSerializer classes:
* They must have a constructor that takes a single {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }} parameter.
* The default for a TypeSerializer when `ToString()` is called on it is to essentially just return whatever the default call to `ToString()` on that type produces.
If a different value should be used, the `string GetToSTringValue(T value, int indentLevel, ToStringFormatProperties formatProperties, bool isPartOfList)` should be overridden.
For the most part, the parameters can be ignored (except for the value) unless some special formatting needs to be done for the particular type (like a byte array being displayed on multiple lines).
See the documentation for <makeLink>ToString</makeLink>.
* The `Serialize` method is pretty straightforward.  The value for that particular property in the class is passed in and the TypeSerializer should return an array of bytes that represents that value.
Generally the various attributes that are part of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }} will help provide information on how the class should actually be serialized
(e.g. Endianness, BCD or not, output length, etc.).
* The `Deserialize` method gets passed the entire array of bytes that make up the message in the `bytes` parameter and the `currentArrayIndex` parameter points to the start of the
value that the `Deserialize` method should be converting into the value.  The `length` parameter is how many bytes the MessageSerializer expects the value to take up from the array.
For many types that are fixed length this value won't be needed but for variable length types the MessageSerializer passing in the length lets you know how many bytes should be processed.
After processing the bytes that the TypeSerializer needs to it should increment the `currentArrayIndex` by how many bytes were processed. The `status` parameter is for indicating any
problems that occurred, especially if they weren't bad enough to require throwing an exception.

TBD: Further methods that can be overridden

The following are the built-in TypeSerializers.

* <makeLink>TypeSerializerBcd</makeLink>
* <makeLink>TypeSerializerByteArray</makeLink>
* <makeLink>TypeSerializerDateTime</makeLink>
* <makeLink>TypeSerializerEnum</makeLink>
* <makeLink>TypeSerializerNumeric</makeLink>
* <makeLink>TypeSerializerSerializableClass</makeLink>
* <makeLink>TypeSerializerString</makeLink>

See <makeLink>TypeSerializerUserDefined</makeLink> for information on how to make your own TypeSelectors.
