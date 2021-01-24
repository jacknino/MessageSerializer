---
layout: defaultWithNavigation
title: User Defined Calculated Fields
---
## {{ page.title }}

It is possible to define your own calculated fields if your protocol has some sort of calculation that is not supported out of the box by the MessageSerializer.

In general, to have your own calculated field type you would need to do the following:
* Create a class that inherits from `CalculatorBase` to be your Calculator for whatever your protocol requires
* Create your own `CalculatedFieldAttributeDefaults` and `CalculatedFieldResultAttributeDefaults`
* Create your own `CalculatedField` and `CalculatedFieldResult` attributes that use your `CalculatedFieldAttributeDefaults` and `CalculatedFieldResultDefaults`

### Example
The example being used here is a bit contrived but the calculation we are going to do with this made up protocol will have the following rules:
* Every message has a 2-byte `Checksum` field as the first field of the message
* There is a field in every message called `NumberNotIncludedInChecksum` that should not be included in the checksum
* The `Checksum` is calculated by adding up all the bytes in the message after the Checksum field except `NumberNotIncludedInChecksum`
and then exclusive-oring the result with 0x5555

Our example message will be:
```
[2 byte Checksum]
[1 byte Message Type]
[1 byte Length]
[4 bytes SomeNumber]
[2 bytes NumberNotIncludedInChecksum]
[4 bytes SomeOtherNumber]
```

For this message:
* As mentioned the `Checksum` field will be our special checksum that adds up all the bytes and then exlusive-ors them with 0x5555
* The `Length` field will be the length of all of the bytes after the `Length` field.
* This particular message has an odd field `NumberNotIncludedInChecksum` that needs to be excluded from the checksum
* The final checksum will be done on the resulting bytes from the `MessageType`, `Length`, `SomeNumber` and `SomeOtherNumber` fields.
* Note that the `Length` needs to be calculated first before the checksum is calculated

The first thing we need to do to implement this is create our Calculator:

```csharp
public class CalculatorTestChecksum5555 : CalculatorBase<ushort>
{
    public override ushort Calculate(params byte[][] arrays)
    {
        ushort checksum = 0;
        foreach (byte[] field in arrays)
        {
            foreach (byte currentByte in field)
            {
                checksum += currentByte;
            }
        }

        return (ushort)(checksum ^ 0x5555);
    }
}
```

At this point we could actually define our class using the standard `CalculatedField` and `CalculatedFieldResult` attributes and call it good:

```csharp
public class TestChecksum5555MessageWithOldAttributes : IMessageSerializable
{
    [CalculatedFieldResult(Name = "ChecksumOldAttribute", Calculator = typeof(CalculatorTestChecksum5555), Priority = 3000, DefaultStart = Position.NextField, DefaultEnd = Position.EndOfMessage)]
    public ushort Checksum { get; set; }
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    public uint SomeNumber { get; set; }
    [CalculatedField(Name = "ChecksumOldAttribute", Exclude = true)]
    public ushort NumberNotIncludedInChecksum { get; set; }
    public uint SomeOtherNumber { get; set; }
}
```

While it is possible to use the existing attributes as above, if they have to be added for every single message in the protocol the `CalculatedFieldResult` especially 
is a bit of a handful.

To make things easier we define our own attributes for this particular scenario:

```csharp
public class CalculatedChecksum5555ResultAttributeDefaults : CalculatedFieldResultAttributeDefaults
{
    public CalculatedChecksum5555ResultAttributeDefaults()
    {
        Name = "Checksum5555";
        Calculator = typeof(CalculatorTestChecksum5555);
        DefaultStart = Position.NextField;
        DefaultEnd = Position.EndOfMessage;
        Priority = 3000;
        Verify = true;
    }
}

public class CalculatedChecksum5555Attribute : CalculatedFieldAttribute
{
    public CalculatedChecksum5555Attribute()
        : base(new CalculatedChecksum5555ResultAttributeDefaults())
    {
    }
}

public class CalculatedChecksum5555ResultAttribute : CalculatedFieldResultAttribute
{
    public CalculatedChecksum5555ResultAttribute()
        : base(new CalculatedChecksum5555ResultAttributeDefaults())
    {
    }
}
```

You can see that defining the attributes is quite easy, the big thing is creating your own `CalculatedFieldResultAttributeDefaults`
that set things up for our particular needs.  Once you've done that we can define our class with the simpler:

```csharp
public class TestChecksum5555MessageWithNewAttributes : IMessageSerializable
{
    [CalculatedChecksum5555Result]
    public ushort Checksum { get; set; }
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    public uint SomeNumber { get; set; }
    [CalculatedChecksum5555(Exclude = true)]
    public ushort NumberNotIncludedInChecksum { get; set; }
    public uint SomeOtherNumber { get; set; }
}
```

The new attributes are much cleaner than our old attributes.  However, since the theory behind our example is that all the messages in this protocol
have the `Checksum` field at the beginning and the `NumberNotIncludedInChecksum` that should be excluded we can actually make things simpler by
creating a {{ site.data.linkVariables["PropertyRule"] }} that will add the attributes for us.  Note that the PropertyRule does check first to make
sure that the class doesn't already have any of our new attributes just in case there is a message that has a special case where the attributes
would need to be manually applied (or if they were applied redundantly on accident).

```csharp
public class PropertyRuleTestChecksum5555 : IPropertyRule
{
    public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
    {
        if (!messageSerializedPropertyInfo.CalculatedFieldAttributes.Any(item => item is CalculatedChecksum5555ResultAttribute || item is CalculatedChecksum5555Attribute))
        {
            if (messageSerializedPropertyInfo.PropertyInfo.Name == "Checksum")
            {
                messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedChecksum5555ResultAttribute());
            }

            if (messageSerializedPropertyInfo.PropertyInfo.Name == "NumberNotIncludedInChecksum")
            {
                CalculatedChecksum5555Attribute attribute = new CalculatedChecksum5555Attribute();
                attribute.Exclude = true;
                messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(attribute);
            }
        }
    }
}
```

To use our new `PropertyRuleTestChecksum5555` we will need to add it to our {{ site.data.linkVariables["SerializerDefaults"] }}
and make sure we load the class using those defaults:

```charp
SerializationDefaults serializationDefaults = new SerializationDefaults();
serializationDefaults.PropertyRules.Add(new PropertyRuleTestChecksum5555());
Serializer.Instance.GetClassInfo(typeof(TestChecksum5555MessageWithNoAttributes), true, serializationDefaults);  
```

Then our class can be defined without any attributes:

```csharp
public class TestChecksum5555MessageWithNoAttributes : IMessageSerializable
{
    public ushort Checksum { get; set; }
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    public uint SomeNumber { get; set; }
    public ushort NumberNotIncludedInChecksum { get; set; }
    public uint SomeOtherNumber { get; set; }
}
```
