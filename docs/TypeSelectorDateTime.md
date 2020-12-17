---
layout: defaultWithNavigation
title: TypeSelectorDateTime
---
## {{ page.title }}

The `{{ page.title }}` checks to see if the `ElementType` of the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}
is `DateTime` and if the `IsBcd` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute is set to true.  

If so, `{{ page.title }}` returns <makeLink>TypeSerializerDateTime</makeLink>.

Note: By default, the <makeLink>PropertyRuleDateTime</makeLink> will set `IsBcd` to true for DateTime properties.

