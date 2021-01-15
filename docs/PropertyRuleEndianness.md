---
layout: defaultWithNavigation
title: PropertyRuleEndianness
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} sets the `Endianness` property of the {{ site.data.linkVariables["MessagePropertyAttribute"] }}
based on a set of rules:

* If the `Endianness` property was explicitly specified then that is what is used.
* If the {{ site.data.linkVariables["MessageClassAttribute"] }} attribute has been specified for the class and `Endianness` was explicitly set for that attribute
then that value is used.
* If {{ site.data.linkVariables["MessagePropertyAttribute"] }} have been set then the `Endianness` from those properties is used.
* Otherwise, `Endiannesses.System` is used.
