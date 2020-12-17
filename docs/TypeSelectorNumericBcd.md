---
layout: defaultWithNavigation
title: TypeSelectorNumericBcd
---
## {{ page.title }}

The `{{ page.title }}` checks to see if the `IsBcd` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute is set to true
and that the `ElementType` of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }} is not `DateTime`.

If so, `{{ page.title }}` returns <makeLinkElementType>TypeSerializerBcd</makeLinkElementType>.
