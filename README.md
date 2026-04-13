# Introduction

NOTE: The documentation is a work in progress and is being integrated into GitHub Pages here: 
[https://jacknino.github.io/MessageSerializer/](https://jacknino.github.io/MessageSerializer/):

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

# Features

The MessageSerializer can help you with a variety of difficulties you might run into when trying 
to serialize/deserialize a byte oriented message.

* Dealing with variable length fields
* Calculating and setting length fields
* Calculating and setting CRCs and other message authentication codes
* Dealing with different data formats like BCD
* Dealing with little/big endianess
* Encrypting messages
* Creating a ToString that will nicely show the fields of the message

