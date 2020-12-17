---
layout: defaultWithNavigation
title: TypeSelector Introduction
---
## Introduction to TypeSelectors

TypeSelectors are what the MessageSerializer uses to determine what <makeLink link="TypeSerializerIntroduction">TypeSerializer</makeLink> class should be used
to process a property on your class.

When the MessageSerializer is processing a class to determine how it should be handled, it will go through each property that
needs to be serialized and check the TypeSelectors that have been configured to determine which <makeLink link="TypeSerializerIntroduction">TypeSerializer</makeLink>
should be used for the property.  This is done by looping through the list of TypeSelectors that have been specified in the <makeLink>SerializationDefaults</makeLink>
that are being used by the Serializer while processing the class.

All TypeSelectors inherit from `ITypeSelector` which only requires implementing one function:

```csharp
Type CheckType(MessageSerializedPropertyInfo propertyInfo)
```

The Type that is returned is of type `TypeSerializerBase<T>` where `T` is the `PropertyType` of the
property that the TypeSerializer is being applied to.  The exception is the case where the property is a `List<SomeType>`.
In that case `T` will be `SomeType` instead.

The TypeSelectors are processed in order until one returns a Type other than `null`.  This can be important as for certain types you may need to make sure that the
order of TypeSelectors will produce the desired result.  See the note on <makeLink>TypeSelectorNumericBcd</makeLink> in the list of default TypeSelectors below.

By default the <makeLink>SerializationDefaults</makeLink> class uses the following TypeSelectors (and in this order)

* <makeLink>TypeSelectorSpecifiedClass</makeLink>
* <makeLink>TypeSelectorSerializable</makeLink>
* <makeLink>TypeSelectorNumericBcd</makeLink>
* <makeLink>TypeSelectorEnum</makeLink>
* <makeLink>TypeSelectorNumeric</makeLink>
* <makeLink>TypeSelectorString</makeLink>
* <makeLink>TypeSelectorDateTime</makeLink>
* <makeLink>TypeSelectorByteArray</makeLink>

See <makeLink>TypeSelectorUserDefined</makeLink> for information on how to make your own TypeSelectors.
