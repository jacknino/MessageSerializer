---
layout: defaultWithNavigation
title: PropertyRuleAuthenticationField
---
## {{ page.title }}

The `{{ page.title }}` {{ site.data.linkVariables["PropertyRule"] }} is intended to be used to make a property that has a name starting with "Crc" 
be automatically set up as an <makeLink>CalculatedAuthenticationResult</makeLink> property.  It uses the following conditions:

* If a property name starts with `Crc` (case-insensitive) and that property does not have an <makeLink>CalculatedAuthentication</makeLink>
or <makeLink>CalculatedAuthenticationResult</makeLink> attribute attached to it the the property has a <makeLink>CalculatedAuthenticationResult</makeLink>
attribute added to it with a `CalculatorType` set to <makeLink>CalculatorAuthenticationCrc16</makeLink>.
* If the property name starts with `Crc` (case-insensitive) and there is not a <makeLink>CalculatedLength</makeLink> or <makeLink>CalculatedLengthResult</makeLink>
attribute attached to the property, a <makeLink>CalculatedLength</makeLink> attribute is added to the property with `Exclude` set to `true`.  This makes it so that
the calculated field will not be included in the length calculation for the message.
