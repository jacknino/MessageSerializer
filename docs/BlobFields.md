---
layout: defaultWithNavigation
title: Blob Fields
---
## Blob Fields (Length/Value Pairs)

Often there will be more than one field in a message that is variable length.  For those fields, there is usually a length field
that is specifically for one particular variable length field.  These length/value pairs in the MessageSerializer are referred to as Blob Fields.

They can be defined by using the `BlobType` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute as shown:

```
[MessagedProperty(BlobType = BlobTypes.Length)]
int BlobLength { get; set; }
[MessageProperty(BlobType = BlobTypes.Data)]
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

