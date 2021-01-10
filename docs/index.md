---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults

layout: defaultWithNavigation
title: Introduction
---
# Introduction

**NOTE: This documentation is a work in progress and is being continually worked on to try
and make using the MessageSerializer as easy as possible. I also apologize for various dead links
and things of that nature that I have not had time to get to yet as well.  There is also a large
TODO list to make the MessageSerializer even more flexible.**

There are a wide variety of protocols in the world that follow a structure in a form similar to:

```
[1 byte MessageType]
[1 byte Length]
[2 bytes SomeField1]
[4 bytes SomeField2]
[vary SomeVariableField]
[2 bytes CRC]
```

Building an array of bytes that matches this structure can be difficult and prone to errors,
especially due to the fact that the size of the data varies depending on what the variable length
field is.  The same holds true when the user has an array of bytes and would like to translate
that into the properties of a class to then handle the message however it needs to be handled.

The MessageSerializer is intended to allow the user to define a class in C# that corresponds 
to a message like the one above.  Then, the MessageSerializer will
do the conversion from the array of bytes into the class and vice-versa, while
at the same time automatically calculating and verifying fields such as lengths and CRC's.

Currently the structure of the message can either be defined purely through the class definition
and a series of attributes applied to the class or through the class definition and an XML definition.

**Note**: The MessageSerializer uses Reflection to determine how to process messages.  
When using the attribute method of loading the class description it uses an unofficial feature
that presumes that when reflecting through a class the properties will be returned in the same
order that they are defined in the message.
.NET does not actually guarantee that will be the case but it seems to hold true in practice.
If this is a concern, use the XML file method of defining your class.

# Small Example

Take this simple message:

```
[1 byte MessageType]
[1 byte Length]
[2 bytes Id]
[vary DeviceName]
[2 bytes Crc]
```

In this message:
* The Length represents the number of bytes in the message after the Length byte
* The DeviceName is a string whose length can be determined by taking the value from the Length byte
and subtracting off the lengths of the Id and CRC16 fields
* The CRC represents a CRC-16 calculation based on all of the bytes before the CRC

To use the MessageSerializer to work with this message you would define a class as follows:

```csharp
public class SampleMessage : IMessageSerializable
{
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    public ushort Id { get; set; }
    public string DeviceName { get; set; }
    public ushort Crc { get; set; }
}
```

Then you can do something like the following:

```csharp
var sampleMessage = new SampleMessage();
sampleMessage.MessageType = 0x23;
sampleMessage.Id = 0x1234;
// "Example" = 0x45, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x0A
sampleMessage.DeviceName = "Example";

byte[] sampleMessageSerialized = Serializer.Instance.Serialize(sampleMessage);
SampleMessage sampleMessageDeserialized = Serializer.Instance.Deserialize<SampleMessage>(sampleMessageSerialized);
```

If you then looked what sampleMessageSerialization held you would see:
`23-09-34-12-45-78-61-6D-70-6C-65-71-B8`

and if you looked at what is in sampleMessageDeserialized you would see:

```
MessageType: 0x23
Length: 0x09
Id: 0x1234
DeviceName: Example
Crc: 0xB871
```

You can see that the MessageSerializer automatically filled in both the Length and the Crc to their appropriate values in the byte array after serialization.
In fact, if you looked at the sampleMessage variable after the serialization you would see that the Length and Crc values now had the correct value as well.

That was all wonderfully easy but you can probably come up with all sorts of questions, such as:
* Those results seem to be using little-endian for numeric values but what if it is supposed to be big-endian?
* What if the Crc is supposed to use a different CRC calculation or is supposed to be a hash?
* What if the DeviceName field is supposed to take up exactly 10 bytes instead of however many the DeviceName string has?
* What if the Length field is supposed to be some weird 3-byte length field?
* What if I receive a message as an array of bytes and want to verify that the CRC is correct in regards to the other bytes that were received?
* That first paragraph in the introduction mentioned attributes.  Where are those?

You can probably come up with plenty of other scenarios.  The MessageSerializer is designed to handle a wide variety of situations
with its build in functionality and does its best to make things work your way with a minimum of effort.  However, if your protocol 
happens to require handling that isn't built in, almost everything can be overridden to work whatever way your protocol requires.

In the following pages, the default functionality, how to change the default functionality and how to add your own functionality
will all be covered, along with examples that will hopefully help you make the MessageSerializer work with your protocol.

In the future it is hoped that there will be a library of definitions for various protocols so that not only does the work not need
to be duplicated by later users but will also provide a wide range of examples for different complicated scenarios.

Let me know if you have questions on how to handle a particular situation with your protocol.
