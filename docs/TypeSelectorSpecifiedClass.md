---
layout: defaultWithNavigation
title: TypeSelectorSpecifiedClass
---
## {{ page.title }}

The `{{ page.title }}` is used when the {{ site.data.linkVariables["MessagePropertyAttribute"] }}
is specified on a property and the `TypeSerializerClass` attribute is set.

`{{ page.title }}` checks to make sure that the type specified in `TypeSerializerClass`
is of type `TypeSerializerBase` and throws an exception if it is not.  Otherwise the type specified
by `TypeSerializerClass` is returned.

