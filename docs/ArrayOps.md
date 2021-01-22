---
layout: defaultWithNavigation
title: ArrayOps
---
## {{ page.title }}

`ArrayOps` is a static class that provides helper functions for dealing with byte arrays and some other related functions.

### Byte Array Functions

```csharp
byte[] GetBytes<TNumericType>(TNumericType value)
```

Gets the bytes that make up the `value`.  The endianness of the result will match the System endianness.

```csharp
byte[] Combine(params byte[][] arrays)
```

Combines the arrays given in `arrays` into a single array.

```csharp
byte[] GetSubArray(byte[] sourceArray, int startIndex, int length, bool reverse = false)
```

Returns the given partial array that starts at `startIndex` of `sourceArray` and is `length` bytes long.
If the resulting array should have the byte order reversed, `reverse` should be set to `true`.

```csharp
string GetHexStringFromArray(byte[] array, int startIndex, int length, bool reverse = false)
```

Returns a string representation of the bytes that are represented in `array` starting from `startIndex`
and for `length` bytes.  If the byte order should be reversed, set `reverse` to `true`.  Similar to
`BitConverter.ToString`.

```csharp
string GetHexStringFromByteArray(byte[] arrayToConvert, string separator)
```

Returns a string representation of the bytes that are represented in `arrayConvert` with `separator`
between each byte.  Similar to `BitConverter.ToString`.

```csharp
byte[] GetBytesFromNumeric<TNumericType>(TNumericType value, Endiannesses requiredEndianness)
```

Gets the bytes that make up the `value`.  The endianness of the returned bytes will match
`requiredEndianness`.

```csharp
byte[] GetBytesFromEnum<TEnumType>(TEnumType value, Endiannesses requiredEndianness)
```

Similar to `GetBytes` except `value` is an `enum`.  The endianness of the returned bytes will match
`requiredEndianness`.

```csharp
TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness, Func<byte[], object> parseFunction)
TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness)
TNumericType GetNumeric<TNumericType>(byte[] fullArray, int startIndex, Endiannesses currentEndianness)
TNumericType GetNumeric<TNumericType>(byte[] array, Endiannesses currentEndianness)
```

Converts the subarray represented by the bytes in `fullArray` starting from `startIndex` of length `length` to a numeric
value of type `TNumericType`.  The returned value will be calculated based on the endianness of the sub-array `currentEndianness`.
The parsing of thes sub-array to `TNumericType` will be done by `parseFunction`.

```csharp
TEnumType GetEnum<TEnumType>(byte[] fullArray, int startIndex, int length, Endiannesses currentEndianness)
```

Gets an enum of the given type given the value represented by the bytes in `fullArray` starting at `startIndex`
for length `length`.  The `currentEndianness` indicates what byte order the bytes are in so that the value
for the enum can be properly determined.

```csharp
ulong GetNumericBcdAsUlong(byte[] bcdArray, int startIndex, int length)
ulong GetNumericBcdAsUlong(byte[] bcdArray)
```

Returns a `ulong` that the BCD value from `bcdArray` represents.  The first function uses `length`
bytes from `bcdArray` starting at `startIndex`.  The second function uses the entire `bcdArray`.

```csharp
TNumericType GetNumericBcd<TNumericType>(byte[] fullArray, int startIndex, int length)
TNumericType GetNumericBcd<TNumericType>(byte[] fullArray, int startIndex)
```

Returns a numeric value of type `TNumericType` that the BCD value from `bcdArray` represents.  The first function uses `length`
bytes from `bcdArray` starting at `startIndex`.  The second function uses the rest of the array starting at `startIndex`.

```csharp
string GetStringFromByteArray(byte[] sourceArray, int startIndex = 0, int count = -1)
```

Returns a string that uses `count` bytes in `sourceArray` starting at `startIndex` and treats them as ASCII values.
`count` being -1 means use the rest of the array.

```csharp
byte[] GetBcdBytes(ulong value, int length, int minLength = 0, int maxLength = -1, bool minimizeLength = false)
```

Gets an array of bytes that represents `value` in BCD.  The length of the array returned is determined using the following:
* The `arrayLength` is set to the maximum of `length` and `minLength` is calculated.
* The `arrayLength` is set to the minimum of the value from the last step and `maxLength` (assuming `maxLength` is not -1)
* The conversion to BCD is done up to `arrayLength` bytes.
* If `minimizeLength` is `true` any initial bytes that are `00` are removed until there are no longer any unneccesary `00` 
bytes or the length has reached `minLength`.

### Conversion Functions

```csharp
 TNumericType Parse<TNumericType>(string stringValue, NumberStyles numberStyle = NumberStyles.None)
 ```

 Converts the `stringValue` into its numeric value and returns it as type `TNumericType`.

 The following TNumericTypes are supported: `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`,
 `long`, `ulong`.

### Endianness Related Functions

```csharp
bool SystemEndiannessIsLittleEndian()
```

Can be used to figure out what the endianness is on the system you are running on.

```csharp
bool EndiannessRequiresReversal(Endiannesses requiredEndianness)
```

Returns true if the `requiredEndianness` is different than the System endianness.

Follows the rules in this chart:

Specified | BitConverter.IsLittleEndian | EndiannessRequiresReversal
---       | ---                         | ---
System    | N/A                         | false
Little    | true                        | false
Little    | false                       | true
Big       | true                        | true
Big       | false                       | false

```csharp
bool EndiannessRequiresReversal(Endiannesses currentEndianness, Endiannesses requiredEndianness)
```

Returns true if the `currentEndianness` would need to be reversed to conform to `requiredEndianness`.

Follows the rules in this chart:

Current   | Required | BitConverter.IsLittleEndian | EndiannessRequiresReversal
---       | ---      | ---                         | ---
System    | System   | N/A                         | false
System    | Little   | true                        | false
System    | Little   | false                       | true
System    | Big      | true                        | true
System    | Big      | false                       | false
Little    | System   | true                        | false
Little    | System   | false                       | true
Little    | Little   | N/A                         | false
Little    | Big      | N/A                         | true
Big       | System   | true                        | true
Big       | System   | false                       | false
Big       | Little   | N/A                         | true
Big       | Big      | N/A                         | false

