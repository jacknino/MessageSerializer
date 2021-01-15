---
layout: defaultWithNavigation
title: MessageClass attribute
---
## {{ page.title }}

The `MessageClass` attribute is used to define behavior that should apply to an entire class that is being defined (unless being overridden on an individual field,
perhaps by a {{ site.data.linkVariables["MessagePropertyAttribute"] }}.

The `MessageClass` attribute supports the following properties:

### DefaultExcludeProperty

The `DefaultExcludeProperty` is a `bool` property used to indicate whether public properties of the class it is being applied to should by default be excluded from the message definition.

`false` means properties will be included by default unless the `Exclude` property of the {{ site.data.linkVariables["MessagePropertyAttribute"] }} is set to `true`.

Default: `false`

### PutInheritedPropertiesLast

The `PutInheritedPropertiesLast` is a `bool` property that can be used to indicate that when your class inherits from another class that should be included in the message definition that the properties
from the class being inherited from should be put at the end of the message.

`false` means that properties of the class being inherited from should be put at the beginning of the message.

Default: `false`

### Endianness

The `Endianness` property indicates what the default endianness for a property should be.  The choices are `System`, `Big` and `Little`.  `System` matches the
endianness of the system the MessageSerializer is running on.

Default: `System`

See also:
* <makeLink>PropertyRuleEndianness</makeLink>
* <makeLink>SerializationDefaults</makeLink>
* {{ site.data.linkVariables["MessagePropertyAttribute"] }}

