---
layout: defaultWithNavigation
title: Strings
---
## Strings

Strings are sent as their ASCII equivalents (currently no Unicode support) but can take a variety of forms
* NULL terminated or not
* Fixed length (NULLs being used for any bytes beyond what the length supports)
* Variable length (based on length bytes)
* Variable length with a maximum length

Here are some general rules
* The Length property of MessageSerializedProperty should be set to make the string field a fixed length
* If a string is shorter than the number of bytes it is allowed to occupy in a fixed length string field any remaining bytes will be NULLs.
* When a string is longer than the length it is allowed to be in a fixed length string field, it will be truncated and will not be null-terminated
* A string is variable length by default or can be be explicitly made variable length by using IsVariableLength = true of MessageSerializedProperty
* If a string is to be variable length, then it should either be the only variable length field in the message or it should be part of a blob pair

