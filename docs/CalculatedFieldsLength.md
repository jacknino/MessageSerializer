---
layout: defaultWithNavigation
title: Length Calculated Fields
---
## {{ page.title }}

"Length" Calculated Fields are a special type of {{ site.data.linkVariables["CalculatedFields"] }} that have build in support in the MessageSerializer.

Many protocols have messages which feature a Length byte that is used to indicate how many bytes are remaining in a message and a Length Calculated Field
can be used to calculate and fill in the Length in your message.

Length Calculated Fields use the `CalculatedLength` and `CalculatedLengthResult` attributes, along with the `CalculatorLength` calculator to specify how 
the length will be calculated and then do the actual calculation.  

`CalculatedLength` and `CalculatedLengthResult` inherit directly from the `CalculatedField` and `CalculatedFieldResult` attributes 
(you can find their definitions under {{ site.data.linkVariables["CalculatedFields"] }}).  `CalculatedLength` and `CalculatedLengthResult` 
don't modify anything about how `CalculatedField` and `CalculatedFieldResult` work except for they use different defaults:

Defaults that change:
* `Name` = "Length"
* `DefaultStart` = `NextField` - this means that the bytes that are included in the length calculation should start at the field after the length field
* `Calculator` = `typeof(CalculatorLength<>)`
* `Priority` = 1000
* `Verify` = `false`

Defaults that stay the same:
* `DefaultEnd` = `EndOfMessage` - this means the length will be calculated with all the fields up to the last one in the message
* `Start` and `End` = `Unspecified` - this means that the `DefaultStart` and `DefaultEnd` will be used for `Start` and `End`
* `Exclude` = `false` - this means that all the fields between `Start` and `End` will be included.

The `CalculatorLength` calculation just computes how many bytes there are in the fields that are to be included in the length field.
However, it is also treated specially by the MessageSerializer as it has a `GetVaryingLengthFieldLength` function that determines
what the length of a variable length field in a message is.  Otherwise when deserializing a message the {{ site.data.linkVariables["TypeSerializer"] }}
for the varying length field wouldn't know how many bytes the field takes up.

**Note:** By default the <makeLink>PropertyRuleLength</makeLink> will consider any property named
`Length` to be the length field for a message using the default CalculatedLengthResult properties.

### Example

To actually use a Length calculated field let's say you have a message with the following definition:

```
[1 byte Message Type]
[1 byte MessageLength]
[2 bytes SomeRandomFieldNotIncludedInLength]
[4 bytes SomeNumber]
[4 bytes SomeOtherNumber]
[vary SomeStringOfVaryingLength]
[1 byte SomeOtherFieldNotIncludedInTheLength]
```

The "fields" are somewhat wordy to make it clear what they are being used for but here are a couple notes anyways:
* We want the `MessageLength` field to be the length field for the message and have that length be automatically calculated
* The fields included in the length are `SomeNumber`, `SomeOtherNumber`, `SomeNullTerminatedStringOfVaryingLength`

The way you could define this message in your code is as follows:

```csharp
public class TestCalculatedLengthMessage : IMessageSerializable
{
    public byte MessageType { get; set; }
    [CalculatedLengthResult]
    public byte MessageLength { get; set; }
    [CalculatedLength(Exclude = true)]
    public ushort SomeRandomFieldNotIncludedInLength { get; set; }
    public uint SomeNumber { get; set; }
    public uint SomeOtherNumber { get; set; }
    public string SomeStringOfVaryingLength { get; set; }
    [CalculatedLength(End = Position.PreviousField)]
    public byte SomeOtherFieldNotIncludedInTheLength { get; set; }
}
```

Some things to take note of:
* The `MessageLength` field was not named `Length` since by default it would automatically be considered the `CalculatedLengthResult` field
which would defeat the purpose of this example
* The `CalculatedLengthResult` attribute on the `MessageLength` field indicates that `MessageLength` is the length field of the message.
* By default, specifying the `CalculatedLengthResult` attribute without any properties being changed means that the length would be calculated
starting with the next field (in this case `SomeRandomFieldNotIncludedInLength`) and go to the last field in the message (`SomeOtherFieldNotIncludedInTheLength`)
* To exclude the `SomeRandomFieldNotIncludedInLength` from the length calculation the `CalculatedLength` attribute was used with `Exclude` set to `true`.  Since the way this message
is structured we could also say that the length calculation should start with `SomeNumber` it would also have been valid to instead use one of the following:
    * Put `[CalculatedLength(Start = Position.NextField)]` on the `SomeRandomFieldNotIncludedInLength` property instead of what we did use
    * Put `[CalculatedLength(Start = Position.ThisField)]` on the `SomeNumber` property and then there wouldn't be a need for a `CalculatedLength` attribute on
    `SomeRandomFieldNotIncludedInLength`
* To exclude the `SomeOtherFieldNotIncludedInTheLength` from the length calculation, `[CalculatedLength(End = Position.PreviousField)]` was used to indicate
that the last field to include in the length calculation should be the field before `SomeOtherFieldNotIncludedInTheLength` (`SomeStringOfVaryingLength`).  The following would also be valid:
    * Put `[CalculatedLength(End = Position.ThisField)]` on `SomeStringOfVaryingLength` and then the `CalculatedLength` attribute wouldn't be necessary
    on `SomeOtherFieldNotIncludedInTheLength`
    * Put `[CalculatedLength(Exclude = true)]` on `SomeOtherFieldNotIncludedInTheLength` instead of what we did put
* If this message was more normal and all the fields after the `MessageLength` field should have been included in the length then only the `CalculatedLengthResult`
attribute would have been needed.


