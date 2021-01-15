---
layout: defaultWithNavigation
title: PropertyRuleLengths
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} tries to apply a smart set of defaults to the properties related to field length
of the {{ site.data.linkVariables["MessagePropertyAttribute"] }}.

* If the `Length` property of the {{ site.data.linkVariables["MessagePropertyAttribute"] }} has not been specified and the type of the property is a numeric type
or an enum type then the `Length` property is set as follows:
  * If `IsBcd` is set then the Length is set to the smallest length that can support the maximum numeric value that the field can hold.  As an example the maximum
  value that a `ushort` type can hold is 65535.  Since this is five digits it would take three bytes in BCD, so the `Length` would be set to three.
  * Otherwise, the `Length` is set to the number of bytes that the type of the property takes up (e.g. byte = 1, short = 2, int = 3, etc.).
* If the `Length` property of the {{ site.data.linkVariables["MessagePropertyAttribute"] }} has not been specified and the type of the property is `IMessageSerializable` then
the `Length` is set to the total length of the fixed length properties of the type of the property.
* If `VariableLength` has not been specified but `BlobType` has been set to `BlobTypes.Data` then `VariableLength` is set to `true`.
* If `VariableLength` has not been specified but the type of the property is a `string` and the `Length` is 0 then `VariableLength` is set to `true`.
* If `MinLength` has not been specified then `MinLength` is set to 0.
* If `MaxLength` has not been specified then `MaxLength` is set to -1 (no max length).
* If `MinimizeVariableLength` has not been specified then `MinimizeVariableLength` is set to `false`.
