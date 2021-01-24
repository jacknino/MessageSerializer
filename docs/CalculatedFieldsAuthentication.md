---
layout: defaultWithNavigation
title: Authentication Calculated Fields
---
## {{ page.title }}

"Authentication" Calculated Fields are a special type of {{ site.data.linkVariables["CalculatedFields"] }} that have build in support in the MessageSerializer.

What is meant by authentication is any sort of field that is intended to ensure that the message has been received properly and hasn't been tampered with, 
whether by accident on purpose.  This could include a CRC, a hash, a checksum or some other method.

Authentication Calculated Fields use the `CalculatedAuthentication` and `CalculatedAuthenticationResult` attributes, along with a calculator to specify how 
the authentication field will be calculated and then do the actual calculation.

`CalculatedAuthentication` and `CalculatedAuthenticationResult` inherit directly from the `CalculatedField` and `CalculatedFieldResult` attributes 
(you can find their definitions under {{ site.data.linkVariables["CalculatedFields"] }}).  `CalculatedAuthentication` and `CalculatedAuthenticationResult` 
don't modify anything about how `CalculatedField` and `CalculatedFieldResult` work except for they use different defaults:

There are a number of built-in calculators for authentication included with the MessageSerializer and it is easy enough to create your own if your protocol
uses a different method.  The included calculators are:
* CalculatorAuthenticationCrc16
* CalculatorAuthenticationCrc32
* CalculatorAuthenticationSha1
* CalculatorAuthenticationSha256
* CalculatorAuthenticationSha512

Defaults that change:
* `Name` = "Authentication"
* `DefaultStart` = `StartOfMessage` - this means that the bytes that are included in the authentication calculation should start with the first field of the message
* `DefaultEnd` = `PreviousField` - this means that the calculation will be done on all the fields up to the one be for the result field
* `Calculator` = this actually remains the default of `null` which means you must provide a `Calculator`
* `Priority` = 2000 - this is bigger than <makeLink>CalculatedFieldsLength</makeLink> because normally you would expect that if there is both a length field
and an authentication field in a message you would want the length to be calculated first and then the authentication (which probably includes the length)
* `Verify` = `true`

Defaults that stay the same:
* `Start` and `End` = `Unspecified` - this means that the `DefaultStart` and `DefaultEnd` will be used for `Start` and `End`
* `Exclude` = `false` - this means that all the fields between `Start` and `End` will be included.

The build-in calculators all take the bytes that should be included in the calculation, perform the calculation and return the result in whatever format that is
appropriate for that calculator (e.g. CRC16 is a `ushort`, CRC32 is a `uint`, SHA1 is a 32-byte array, etc.).  Note that the definition of the result property 
should have the correct type that the result is expected to return.

**Note:** By default the <makeLink>PropertyRuleAuthentication</makeLink> will consider any property named
`Crc` to be a authentication result field for a message using the default CalculatedAuthenticationResult properties with the calculator being
`CalculatorAuthenticationCrc16`.

### Example

To actually use a Authentication calculated field let's say you have a message with the following definition:

```
[1 byte Message Type]
[1 byte Length]
[2 bytes FirstFieldIncludedInChecksum]
[4 bytes SomeNumber]
[2 bytes NumberNotIncludedInChecksum]
[4 bytes SomeOtherNumber]
[2 byte Checksum]
```

The "fields" are somewhat wordy to make it clear what they are being used for but here are a couple notes anyways:
* The `Length` field is the length of all the bytes after the length field including the `ChecksumThatIncludesItselfInCalculation`.
Since it is named `Length` the MessageSerializer will automatically consider it a Length field so it doesn't need any
<makeLink>CalculatedFieldsLength</makeLink> related attributes.
* The `Checksum` will be our authentication result field and will just be a ushort representing
the total from adding up all the bytes included in the checksum.
* The fields whose bytes should be included in the checksum are `FirstFieldIncludedInChecksum`, `SomeNumber` and
`SomeOtherNumber`.

The first thing we need to do is define a Calculator to actually perform the checksum.  You can do this by creating a class
that inherits from `CalculatorBase` and implementing the `Calculate` method:

```csharp
public class CalculatorTestChecksum : CalculatorBase<ushort>
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

        return checksum;
    }
}
```

Then you can define your class to represent your message:

```csharp
public class TestCalculatedAuthenticationMessage : IMessageSerializable
{
    public byte MessageType { get; set; }
    public byte Length { get; set; }
    [CalculatedAuthentication(Start = Position.ThisField)]
    public ushort FirstFieldIncludedInChecksum { get; set; }
    public uint SomeNumber { get; set; }
    [CalculatedAuthentication(Exclude = true)]
    public ushort NumberNotIncludedInChecksum { get; set; }
    public uint SomeOtherNumber { get; set; }
    [CalculatedAuthenticationResult(Calculator = typeof(CalculatorTestChecksum))]
    public ushort Checksum { get; set; }
}
```

Some things to take note of:
* The `CalculatedAuthenticationResult` attribute on the `Checksum` field indicates that `Checksum` is the property where the checksum calculation should go.
* Note that the `Calculator` was set to `typeof(CalculatorTestChecksum)` so that our calculator for calculation the checksum will be used.
* By default, specifying the `CalculatedAuthenticationResult` attribute with just the Calculator means that the fields to be included would be all the fields in the message
up to (but not including) the `Checksum` field.
* To make it so that only the fields starting with `FirstFieldIncludedInChecksum` are included in the calculation the [CalculatedAuthentication(Start = Position.ThisField)]
was added to change where the fields should start. The following would also have been valid (although wouldn't necessarily be as clear):
    * Put `[CalculatedAuthentication(Start = Position.NextField)]` on the `Length` property instead of what we did use
    * Put `[CalculatedAuthentication(Start = Position.PreviousField)]` on the `SomeNumber` property instead of what we did use
    * Put `[CalculatedAuthentication(Exclude = true)]` on `MessageType` and `Length`
* To exclude the `NumberNotIncludedInChecksum` from the checksum calculation, `[CalculatedAuthentication(Exclude = true)]` was used to indicate
that the field shouldn't be included.


