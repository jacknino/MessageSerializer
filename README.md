# Introduction

The MessageSerializer is intended to allow the user to define a class in C# that corresponds 
to a message that will actually be sent/received as a string of bytes.  The MessageSerializer
is designed to do the conversion from the string of bytes into the class and vice-versa.

Currently the structure of the message can either be defined purely through the class definition
and a series of attributes or through an XML definition.

**Note**: The MessageSerializer uses Reflection to determine how to process messages.  
When using the attribute method of loading the class description it uses an unofficial feature
that presumes that when reflecting through a class the properties will be returned in order.  .NET does not actually guarantee
that will be the case but it seems to hold true in practice.  If this is a concern, use the XML
file method of defining your class

# Example

As a very simple example take this example message:

`[1 byte MessageType][1 byte Length][2 bytes Id]`

We will say the MessageType is 0x23 and the Id is 0x1234.
The length is the length of the rest of the message, so 2.
For this example we will say that multi-byte values are sent LSB.

In that case the message being sent would be:

`[0x23][0x02][0x34][0x12]`

To declare a class for this message you could use:

```
public class SampleMessage : IMessageSerializable
{
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    public ushort Id { get; set; }
}
```

Then to use the class:

```
using MessageSerializer;

SampleMessage sampleMessage = new SampleMessage();
sampleMessage.MessageType = 0x23;
sampleMessage.Id = 0x1234;
			
byte[] sampleMessageSerialized = Serializer.Instance.Serialize(sampleMessage);
SampleMessage sampleMessageDeserialized = Serializer.Instance.Deserialize<SampleMessage>(sampleMessageSerialized);
```

A couple things to note:
* The SampleMessage class inherits from IMessageSerializable.  That is currently required but will be removed in the future as it isn't providing any functionality and is unnecessary.
* Currently the fields for the messages must be defined as Properties in the class definition.  This will be expanded in the future to support fields and possibly functions.
* There aren't any attributes on the properties.  The MessageSerializer does it's best to figure out as much as it can just from the definition of the properties, such as:
  * Numeric fields default to being the size of the variable being used to store them
  * Properties named Length are expected to be the Length field and default to it representing the rest of the bytes in the message
* The MessageSerializer will figure out what a Length field should be set to and will set it automatically.
* When you Serialize or Deserialize using a particular class and you haven't purposely loaded it beforehand, its first usage will cause the MessageSerializer to process the class and configure how that class should be used

# How To Define Properties

This section is intended to outline how to define different types of Properties and the options you can use to affect 
how they are interpreted when serializing or deserializing.

Because the XML file format essentially recreates the Property/Attribute format when using the attributes to configure
how a class should be processed, for the most part the instructions here will only mention the attributes.

## Standard Numeric Fields

As a general rule if have a numeric property you should just declare the property to be of the type that fits
with the data in the message.

If you have a 1 byte value you normally would define the property as a `byte`, a 2-byte field would either be
a `short` or `ushort`, etc.

**TODO: Currently this feature is broken so numeric types need to be defined as the same type as fits the output size**
If for some reason you don't want the number of bytes that the numeric value takes up in the message to match the number
of bytes that the numeric type takes up you can use the Length property of the MessageSerializedProperty attribute to
indicate how many bytes the value should actually take up

```
[MessageSerializedProperty(Length = 10)]
ushort Takes10Bytes { get; set; }
```

## BCD Numeric Fields

Numeric values that should be sent as BCD work the same as regular numerics except for the length
that they take up is not just the size of the variable but the minimum number of bytes that it would
take to hold the maximum value that the numeric type could hold.

As an example, since the maximum value a `ushort` could hold is 65535, this requires at least two and a half bytes.
Therefore, the number of bytes used would be 3 by default.

These end up being the default lengths:
* byte = 2 bytes
* ushort = 3 bytes
* uint = 5 bytes
* ulong = 10 bytes

Note that it doesn't make sense to use a signed value for a BCD property since BCD doesn't have the concept
of a negative number.

There are two ways to indicate that a property should be treated as BCD.  You can either start the name
of the property with Bcd, like BcdMeterValue or you can set the IsBcd property of MessageSerializedProperty 
to true.

Note that if you start the name of a property with Bcd but it isn't supposed to be treated as Bcd you must set IsBcd to false.

## Enums

Enums are essentially treated the same way as a standard numeric type matching the underlying numeric type that holds the enum 
and the value of the enum is what will be serialized into the message.

## Date/Time

DateTime types are serialized based on 3 parameters:
* Format (defaults to `MMddyyyyHHmmss`) and conforms to the ToString specification for DateTime types
* IsBcd (defaults to true for DateTime types) determines if the resulting value is serialized as BCD or as a number.
Currently only IsBcd = true is supported
* Length (defaults to the length of Format / 2) determines the final length of the value.  Currently only the default is supported.

## Strings

Strings are sent as their ASCII equivalents (currently no Unicode support) but can take a variety of forms
* NULL terminated or not
* Fixed length (NULLs being used for any bytes beyond what the length supports)
* Variable length (based on length bytes)
* Variable length with a maximum length

Here are some general rules
* The Length property of MessageSerializedProperty should be set to make the string field a fixed length
* If a string is shorter than the number of bytes it is allowed to occupy in a fixed length string field any remaining bytes will be NULLs.
* When a string is longer than the length it is allowed to be in a fixed length string field, it will be truncated and will not be null-terminated
* A string is variable length by default or can be be explicitly made variable length by using IsVariableLength = true of MessageSerializedProperty
* If a string is to be variable length, then it should either be the only variable length field in the message or it should be part of a blob pair

## Length Bytes

Many messages are not fixed in length and therefore have a length property.  The MessageSerializer is designed so that rather than
the user's code having to determine how long a message is in order to fill in the length property the MessageSerializer will calculate
the length and not only include it in the serialized bytes but set the value in the class structure.

Here are some rules for `Length` properties:
* There can only be one `Length` property per message
* A property can be excluded from the length calculation when it otherwise would have been included by setting the
`ExcludeFromLength` property of the `MessageSerializedProperty` attribute to `true`.
* There are three length types that are supported: `EntireMessage`, `RestOfMessageIncludingLength` and `RestOfMessage`.
The `Length` property can be designated by setting the `MessageLengthType` property of the `MessageSerializedProperty` attribute
on the appropriate property to one of the three values.  The meaning of the values are as follows:
  * `EntireMessage` sets the length to the number of bytes in the entire message except for those bytes that have been specifically excluded.
  * `RestOfMessageIncludingLength` sets the length to the number of bytes that occur starting from the `Length` field until the
  end of the message except for those bytes that have been specifically excluded.
  * `RestOfMessage` is the same as `RestOfMessageIncludingLength` except the `Length property itself is not included in the 
  length calculation.
* By default, if a property is named `Length` it will be assumed to be a length field of type `RestOfMessage` as long as there
isn't another property that is specifically designated as the `Length` property.  If a property meets these conditions but 
shouldn't actually be considered the `Length` property it should have its `MessageLengthType` property of the
`MessageSerializedProperty` attribute set to `None`.

## Blob Fields (Length/Value Pairs)

Often there will be more than one field in a message that is variable length.  For those fields, there is usually a length field
that is specifically for the variable length field.  These length/value pairs in the MessageSerializer are referred to as Blob Fields.

They can be defined by using the `BlobType` property of the `MessageSerializedProperty` attribute as shown:

```
[MessageSerializedProperty(BlobType = BlobTypes.Length)]
int BlobLength { get; set; }
[MessageSerializedProperty(BlobType = BlobTypes.Data)]
string BlobData { get; set; }
```

Some notes:
* Similar to `Length` properties in a message the MessageSerializer will compute and automatically fill in the
Blob Length property.
* The Data field does not actually have to be a variable length type and it can also be an IMessageSerializable
class or a List of some type (and thus actually contain multiple fields).
* There can be any number of Blob Fields in a message.
* If a Blob Length field is detected, by default the next encountered Blob Data property is expected to be the Data 
associated with that Length. If a different property should be used the `AssociatedBlobProperty` property of the
`MessageSerializedProperty` of the Blob Length property should be set to the name of the property that the data
should be associated with.

As an example of the `AssociatedBlobProperty` in the following example class, `MeterSize2` will be associated with `BcdValue2`
and `MeterSize` will be associated with `BcdValue` even though neither Length/Value pair appear next to each other in the message
and they occur in a different order in the message.

```
public class DoubleGapBlob : IMessageSerializable
{
	[MessageSerializedProperty(BlobType = BlobTypes.Length, AssociatedBlobProperty = "BcdValue2")]
	public byte MeterSize2 { get; set; }

	[MessageSerializedProperty(BlobType = BlobTypes.Length)]
	public byte MeterSize { get; set; }

	public int SomeOtherField { get; set; }
	public short OtherField { get; set; }

	[MessageSerializedProperty(BlobType = BlobTypes.Data)]
	public ulong BcdValue { get; set; }

	[MessageSerializedProperty(BlobType = BlobTypes.Data)]
	public int BcdValue2 { get; set; }
}
```

## Message Authentication

The MessageSerializer supports automatically calculating message verification codes from the bytes of a message.

There are multiple built in verification methods:
* CRC16 (should have a type of ushort) - AuthenticationCrc16
* CRC32 (should have a type of ulong) - AuthenticationCrc32
* SHA-1 (should have type of byte[]) - AuthenticationSha1
* SHA-256 (should have type of byte[]) - AuthenticationSha256
* SHA-512 (should have type of byte[]) - AuthenticationSha512

To compute an authentication value create a field and set the `AuthenticationClass` property of the `MessageSerializedProperty` attribute 
to the Type of the class that has the calculation mechanism you want to use.  The class should be of type `AuthenticationBase`
and the type of the property that will hold the value should be of the same type that the `AuthenticationClass` is designed to
return.

When there is an authentication field, it is expected that all of the fields in the message (except the Authentication field itself)
will be included in the calculation unless you mark them with `ExcludeFromAuthentication = true` in the `MessageSerializedProperty` attribute.

Here is a simple example:
```
public class

Note: If the name of a property starts with Crc it will default to being a CRC16 field and defaults to being excluded from the calculation

## Lists

## Sub-classes

## Excluding Property

If you want to exclude a public property that would otherwise be included in serialization/deserialization
set the `Exclude` property of the `MessageSerializedProperty` attribute to `true`.
