---
layout: defaultWithNavigation
title: PropertyRuleBcd
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} checks to see if the property name starts with `Bcd` (case-insensitive).  If it does and the
`IsBcd` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute has not been explicitly set for the property then `IsBcd` is set to `true`.
