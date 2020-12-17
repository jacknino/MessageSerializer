---
layout: defaultWithNavigation
title: Numeric Fields
---
# {{ page.title }}

In this case, by numeric fields we are talking about the integer types that are built in to C#:
`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long` and `ulong`.

As a general rule if have a numeric property you should just declare the property to be of the type that fits
with the data in the message.

If you have a 1 byte value you normally would define the property as a `byte`, a 2-byte field would either be
a `short` or `ushort`, etc.

**TODO: Currently the below feature is broken so numeric types need to be defined as the same type as fits the output size**
If for some reason you don't want the number of bytes that the numeric value takes up in the message to match the number
of bytes that the numeric type takes up you can use the Length property of the MessageSerializedProperty attribute to
indicate how many bytes the value should actually take up

```
[MessageSerializedProperty(Length = 10)]
ushort Takes10Bytes { get; set; }
```

## Defaults

| Type | Value |
| --- | --- |
| Type Selector | <makeLink>TypeSelectorNumeric</makeLink> |
| Default Type Serializer | <makeLink>TypeSerializerNumeric</makeLink> |
