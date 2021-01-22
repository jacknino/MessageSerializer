---
layout: defaultWithNavigation
title: Calculated Fields Introduction
---
## {{ page.title }}

Calculated fields are fields in a message that are calculated based on other fields in your message.  This can
include a length field that shows how many bytes are left in the message or a CRC that is calculated on all the
bytes in the message before the CRC field.

The MessageSerializer uses the `CalculatedField` and `CalculatedFieldResult` attributes to determine what fields
should be included in the calculation, how to perform the calculation and where to put the result.  The calculation
is done using types that inherit from `CalculatorBase`.
Calculated fields can be given a priority so that if you have multiple calculated fields they can be calculated 
in the proper order.  An example of this would be if you had a length field whose value needs to include the length
of the CRC field but the CRC includes the length in the CRC calculation.  In this instance you would want to
make sure the length was calculated first and then the CRC.

When you Serialize a message with the calculated field the calculated value will not only be included in the bytes
representing your message but the property for that field will also be set in your class object.

In a similar manner, when you Deserialize a message, the property will be set with the value received in the message
and also the calculated field value can be verified to make sure that the value received is what was expected.

The MessageSerializer has two types of built in calculated fields:
* <makeLink link="CalculatedFieldsLength">Length</makeLink> - For calculating length fields
* <makeLink link="CalculatedFieldsAuthentication">Authentication</makeLink> - For calculating authentication values like CRCs and Hashes.
A number of Calculators are already included for various types of calculation.

You can also make your own <makeLink link="CalculatedFieldsUserDefined">User Defined</makeLink> calculated field types.

### CalculatedField Attribute

The `CalculatedField` attribute is used on the properties of your class that are involved with the calculation.  As you'll
see from the properties of `CalculatedField` you shouldn't have to put a `CalculatedField` attribute on every property of
your class that is going to be part of the calculation. In many situations you might only need the `CalculatedFieldResult` property.

`CalculatedField` has the following properties:

#### Name

The `Name` of a `CalculatedField` is used to indicate which calculated field this attribute is referring to.
If you don't specify the name, the name will just be an empty string, which is totally reasonable if you only have one calculated
field in your message.  Otherwise, each calculated field needs to have its own name and any attributes associated with it
should specify the same name.

Default: Empty string

#### Exclude

`Exclude` indicates that this particular property should be excluded from the calculation.  Generally, this shouldn't be needed
unless the range of fields specified by `Start` and `End` (or `DefaultStart` and `DefaultEnd` in `CalculatedFieldResult`) in your calculation 
should exclude one or more of the fields in the middle of that range.

Default: `false`

#### Start

`Start` is a `Position` enum used to indicate where the start of the properties that should be included in the calculation is in relation to the
property you are applying to this attribute is.

`Position` contains the following values:
* Unspecified - Meaning that this `CalculatedField` attribute is not intended to indicate anything about the range of fields to include in the calculation
* StartOfMessage - Indicates the first field in the message
* ThisField - Indicates the field that this attribute is being applied to
* NextField - Indicates the field after this field
* PreviousField - Indicates the field before this field
* EndOfMessage - Indicates the last field in the message

Default: `Unspecified`

#### End

`End` is essentially the same as `Start` except it indicates the end of the range of fields to include.  It uses the same `Position` enum so 
the choices for values are the same as in `Start`.

Default: `Unspecified`

### CalculatedFieldResult attribute

The `CalculatedFieldResult` attribute inherits from `CalculatedField` so all of the properties mentioned there are still available.
Beyond that, for each calculated field and its set of `CalculatedField` attributes with the same `Name` there should only be one `CalculatedFieldResult`
attribute.

`CalculatedFieldResult` has the following properties:

#### Calculator

This is the `Type` of the class inheriting from `CalculatorBase` (see below) that should be used to perform the actual calculation.

Default: `null` (which isn't valid so make sure to specify a valid `Type`)

#### Priority

The `Priority` is an `int` property used for determining in what order calculated fields should have their calculations done.  They will be calculated
in order from lowest priority to highest priority.  If two calculated fields have the same priority they will be calculated in the order
they are encountered in the message.

Default: `0`

#### DefaultStart

`DefaultStart` is the `Start` position that will be used if no property has the `Start` property set to something other than `Unspecified`.
See `Start` under `CalculatedField` above.

Default: `StartOfMessage`

#### DefaultEnd

`DefaultEnd` is the `End` position that will be used if no property has the `End` property set to something other than `Unspecified`.
See `End` under `CalculatedField` above.

Default: `EndOfMessage`

#### Verify

`Verify` is a `bool` value that indicates when deserializing a message with this calculated field if the value received matches what is
calculated based on the fields in the message.

Default: `false`

### CalculatorBase

Any `Calculator` that is used with a calculated field must inherit from `CalculatorBase`.

`CalculatorBase` is a template type that has a parameter of `TCalculatorResultType`.  `TCalculatorResultType` is the
type that the calculation returns.

There is only one function that has to be implemented:

```charp
TCalculatorResultType Calculate(params byte[][] arrays)
```

The `arrays` parameter is the bytes that should be included in the calculation.  When `Calculate` is called the calculation
should be done on the bytes in `arrays` and the result returned.

