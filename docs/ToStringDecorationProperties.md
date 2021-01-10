---
layout: defaultWithNavigation
title: ToStringDecorationProperties
---
## {{ page.title }}

`ToStringDecorationProperties` are used by <makeLink>ToStringFormatProperties</makeLink> and the {{ site.data.linkVariables["Serializer"] }} `ToString` method 
to determine how the output data from `ToString` should be formatted for the different fields of the message.

### IndentString

`IndentString` is the string that is used whenever an indent is needed (e.g. a tab, two spaces, four spaces, etc.)

Default: Four spaces

### SeparateLine

`SeparateLine` indicates whether each item (such as a field in the message, or an item in a list) should be on a separate line.

Default: `true`

### Indent

`Indent` indicates whether each item after the first should be indented from the first item's indent level

Default: `true`

### NameValueSeparator

`NameValueSeparator` is the string that should go between the name and the value of whatever is being logged.

Default: `": "`

### Prefix

`Prefix` is the string that should go before each item (such as a name/value pair).

Used with `Suffix` you could make it so that each item could be formatted something like `<ItemName: Value>`

Default: `""`

### Suffix

`Suffix` is the string that should go after each item (such as a name/value pair).

Used with `Prefix` you could make it so that each item could be formatted something like `<ItemName: Value>`

Default: `""`

### Separator

`Separator` is the string that should be used between each item.

Default: `", "`

