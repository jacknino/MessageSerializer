---
layout: defaultWithNavigation
title: PropertyRuleDateTime
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} sets various defaults on the {{ site.data.linkVariables["MessagePropertyAttribute"] }}
for properties with a type of `DateTime`:

* If `IsBcd` had not been explicitly specified it is set to `true`.
* If `Format` has not been explicitly specified it is set to `"MMddyyyyHHmmss"`.
* If `Length` has not been explicitly specified it is set to the half of the length of the `Format` string (`Format.Length / 2`).
