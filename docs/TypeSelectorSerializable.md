---
layout: defaultWithNavigation
title: TypeSelectorSerializable
---
## {{ page.title }}

The `{{ page.title }}` checks to see if the `ElementIsMessageSerializableObject` property of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}
is true.  If so, `{{ page.title }}` returns <makeLinkElementType>TypeSerializerSerializableClass</makeLinkElementType>.

Note: This essentially means that the `ElementType` of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }} implements `IMessageSerializable`.

