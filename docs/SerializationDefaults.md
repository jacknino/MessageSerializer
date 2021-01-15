---
layout: defaultWithNavigation
title: SerializationDefaults
---
## {{ page.title }}

The `SerializationDefaults` is the class used to give the MessageSerializer tips on how to handle your class.  These defaults can make 
it so that you when you go to define the classes for your messages you will need a minimum of attributes and information of that nature
on the actual classes.

### Endianness

There are three types of Endianness the MessageSerializer supports: `Little`, `Big` and `System`.  `Little` and `Big` are self-explanatory.
`System` means to use whatever the system the MessageSerializer is running on.

When determining what Endianness to use for a field in a message the following priority is used:
* If specified, the `Endianness` from the {{ site.data.linkVariables["MessagePropertyAttribute"] }} on the property for the field in your class.
* If specified, the `Endianness` from the {{ site.data.linkVariables["MessageClassAttribute"] }} on your class.
* The `Endianness` from the `SerializationDefaults`

Default: `System`

### TypeSelectors

`TypeSelectors` is a list of type `TypeSelectorBase` that is iterated through for each property on your class to determine the {{ site.data.linkVariables["TypeSerializer"] }}
that should be used to process that field of your message. See {{ site.data.linkVariables["TypeSelector"] }} for more information.

### PropertyRules

`PropertyRules` is a list of type `IPropertyRule` that is iterated through for each property on your class to determine the PropertyRule(s)
that should be used for that field of your message. See {{ site.data.linkVariables["PropertyRule"] }} for more information.

