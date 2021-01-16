---
layout: defaultWithNavigation
title: MessageProperty attribute
---
## {{ page.title }}

The `MessageProperty` attribute is used to define behavior that should apply to an individual property that is being defined on a class that represents a message in your protocol.

The `MessageProperty` attribute supports the following properties:

### Exclude

`Exclude` is a `bool` property that indicates whether or not the property it is being applied to should be included as part of the message serialization.

Default: `false`

### Endianness

The `Endianness` property indicates what the endianness for a property should be.  The choices are `System`, `Big` and `Little`.  `System` matches the
endianness of the system the MessageSerializer is running on.

Default: `System`

See also:
* <makeLink>PropertyRuleEndianness</makeLink>
* <makeLink>SerializationDefaults</makeLink>
* {{ site.data.linkVariables["MessageClassAttribute"] }}

### Prepad

`Prepad` is a `bool` property that is used to indicate that when a property (such as a string) has a specified length but the actual length of the value is
less than the required value, the value should be prepadded by `PrepadCharacter` to make the length the correct length.

Currently the only {{ site.data.linkVariables["TypeSerializer"] }} that uses Prepad is <makeLink>TypeSerializerString</makeLink>

Default: `false`

### PrepadCharacter

When `Prepad` is `true` and characters are being prepended, the character to use.

Default: `0` (through <makeLink>PropertyRulePrepad</makeLink>)

### BlobType

Used for {{ site.data.linkVariables["Blob"] }} fields.

When a property should be considered the length field for a `Blob`, should be set to `BlobTypes.Length`.
When a property should be considered the data field for a `Blob`, should be set to `BlobTypes.Data`.
When the property is not part of a `Blob` should be set to `BlobTypes.None`

Default: `None`

### AssociatedBlobProperty

Only used when `BlobType` is set to `Length` for a {{ site.data.linkVariables["Blob"] }}.  Normally the 
next property that is encountered that has a `BlobType` of `Data` will be considered the associated blob data
for the property that has `BlobType` of `Length`.  If this does not work then `AssociatedBlobProperty` should be
set to the name of the property that should be considered the `Data` associated with this `Length` property.

### IsBcd

`bool` property to indicate whether or not a numeric field should be serialized/deserialized as BCD.

Default: `false`

Note: By default, when using <makeLink>PropertyRuleBcd</makeLink> a numeric property whose name starts with
`Bcd` will be considered a BCD field.

### TypeSerializerClass

If for some reason the {{ site.data.linkVariables["TypeSelector"] }}s that are configured will not work for 
figuring out what {{ site.data.linkVariables["TypeSerializer"] }} should be used for a particular property
the `TypeSerializerClass` can just be set to the `Type` of the TypeSerializer that should be used.

Default: `null` (meaning the TypeSerializer is determined using the TypeSelectors)

### Length

When a property should be a specific length, the `Length` property should be set to that length.  This is
generally only useful for a property that doesn't have a defined length (e.g. a `string`).  Types
that have a fixed length, like an `int` do not need to have the `Length` specified.

Default: `0` (meaning Length not specified)

### VariableLength

`bool` value to indicate that this field should be considered variable length

Default: `false`

### MinLength

This property is used to indicate for a variable length field what the minimum length of the field should be.

Default: `0`

### MaxLength

This property is used to indicate for a variable length field what the maximum length of the field should be.

Default: `-1` (meaning no maximum)

### MinimizeVariableLength

Mainly for numeric fields.  If a numeric field is variable length you could get a value like 0x00000012.  
That value only needs one byte to be represented but will take up 4 by default.  If `MinimizeVariableLength` 
is set to `true` the field will take up the minimum bytes necessary as defined by `MinLength`.

Default `false`

### Format

`Format` is a `string` type that is used to indicate how a particular type should be formatted.  By default
Format is only used by <makeLink>TypeSerializerDateTime</makeLink> to determine how the `DateTime` should be
formatted in the message.

Default: `""` (empty string)
