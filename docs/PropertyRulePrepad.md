---
layout: defaultWithNavigation
title: PropertyRulePrepad
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} is a simple rule that just checks to see if a `PrepadCharacter` has been specified for 
the {{ site.data.linkVariables["MessageProperty"] }} attribute on a property.  If not, the `PropertyRulePrepad` will set the `PrepadCharacter` to `'0'`.

Note that setting the `PrepadCharacter` does not automatically cause it to be used.  For `PrepadCharacter` to be used, the `Prepad` property on the {{ site.data.linkVariables["MessageProperty"] }}
attribute must be set to `true`.
