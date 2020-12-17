---
layout: defaultWithNavigation
title: TypeSelectorNumeric
---
## {{ page.title }}

The `{{ page.title }}` checks to see if the `ElementType` of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}
is a numeric type and that the length as specified by the `Length` property of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }} attribute
is a power of 2.  If so, `{{ page.title }}` returns <makeLinkElementType>TypeSerializerNumeric</makeLinkElementType>.

Note: In the future the length requirement will most likely be removed.
