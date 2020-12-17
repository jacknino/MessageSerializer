---
layout: defaultWithNavigation
title: BCD Numeric Fields
---
## BCD Numeric Fields

Numeric values that should be sent as BCD work the same as regular numerics except for the length
that they take up is not just the size of the variable but the minimum number of bytes that it would
take to hold the maximum value that the numeric type could hold.

As an example, since the maximum value a `ushort` could hold is 65535, this requires at least two and a half bytes.
Therefore, the number of bytes used would be 3 by default.

These end up being the default lengths:
* byte = 2 bytes
* ushort = 3 bytes
* uint = 5 bytes
* ulong = 10 bytes

Note that it doesn't make sense to use a signed value for a BCD property since BCD doesn't have the concept
of a negative number.

There are two ways to indicate that a property should be treated as BCD.  You can either start the name
of the property with Bcd, like BcdMeterValue or you can set the IsBcd property of MessageSerializedProperty 
to true.

Note that if you start the name of a property with Bcd but it isn't supposed to be treated as Bcd you must set IsBcd to false.
