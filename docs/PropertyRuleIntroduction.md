---
layout: defaultWithNavigation
title: PropertyRule Introduction
---
## {{ page.title }}

PropertyRules are used to provide a mechanism to assign defaults to properties of a class in order to lessen
the number of attributes that have to be applied when designing a particular class.  

As an example, one of the built-in property rules is <makeLink>PropertyRuleBcd</makeLink>.  Normally, if you want to indicate a property is in BCD format
you would define it as:

```csharp
[MessageProperty(IsBcd = true)]
public int SomeBcdField { get; set; }
```

However, the <makeLink>PropertyRuleBcd</makeLink> is designed to check if a property name starts with `Bcd` and if it does and IsBcd has not been 
explicitly set by a {{ site.data.linkVariables["MessageProperty"] }} attribute on the property, then <makeLink>PropertyRuleBcd</makeLink> will set IsBcd to true.  This means that
all you need to do to have a BCD field is just make sure the name of the property starts with `Bcd` (the case of the Bcd doesn't matter).

```csharp
public int BcdSomeField { get; set; }
```

If for some reason you then had a property that you really wanted the name to start with `Bcd` but it is not a Bcd field then you 
can just set `IsBcd` to `false` and then the <makeLink>PropertyRuleBcd</makeLink> won't be applied.

```csharp
[MessageProperty(IsBcd = false)]
public int BcdNotReally { get; set; }
```

To specify the PropertyRules to use for your classes they should be specified in the `PropertyRules` list of the {{ site.data.linkVariables["SerializationDefaults"] }}
that are being used for your class.  The defaults mentioned below are added to the `PropertyRules` by default but you are free to modify the list or add to it as needed.
The {{ site.data.linkVariables["SerializationDefaults"] }} can be specified when you call `LoadSerializableClassesFromAssembly`, `LoadSerializableClassesFromSettings` or `GetClassInfo` on the 
{{ site.data.linkVariables["Serializer"] }} class instance you are using.  Note that if you don't call one of those functions first before you call `Serializer` or `Deserialize` the
default {{ site.data.linkVariables["SerializationDefaults"] }} will be used.

By default, the following PropertyRules are used:

* <makeLink>PropertyRuleEndianness</makeLink>
* <makeLink>PropertyRuleLengthField</makeLink>
* <makeLink>PropertyRuleBcd</makeLink>
* <makeLink>PropertyRuleAuthenticationField</makeLink>
* <makeLink>PropertyRuleDateTime</makeLink>
* <makeLink>PropertyRuleLengths</makeLink>
* <makeLink>PropertyRulePrepad</makeLink>
