---
layout: defaultWithNavigation
title: TODO List
---
## {{ page.title }}

This page is intended to be a TODO list of items that are already planned to be included in the MessageSerializer.

Currently, this list is not intended to be in priority order

* Improve/complete documentation
  * Add documentation for the MessageSerializerClassFileCreator
* Add support for fields in classes instead of just properties.
  * Setting for whether or not to load field/properties/both/etc.
* Add support for serializing/deserializing to a protected/private field/property if necessary
* Add support for loading class descriptions from JSON
* Support encryption/decryption (Note that CalculatedFields should be able to support encryption and decryption right now but it has not been tested)
* Remove requirement for classes to inherit from IMessageSerializable
* Add support for signing the output DLL
* Add more defaults to SerializationDefaults
* Support loading classes by namespace
* Support loading classes by particular type
* Add support for constant fields (e.g. MessageType will always be 0x26 for some class for a protocol)
* Support for emumerable types instead of List<> in particular
* Make it an option for whether the default length for BCD fields is based on the size of the numeric type containing it or the max size of the BCD value 
(e.g. a `short` value could have a length of 2 bytes because that is the length of a short or it could have a length of 3 bytes because the max value is 65535 and 3 bytes are needed to hold that value)
* For ToString, be able to override things on a per item basis (at least some of it), like maybe the Name to use, whether or not to show hex, put new items on a line, etc.
* Allow generated classes to inherit from a different base class than SerializerBase
* Support loading all attributes that are on a property/field in case someone has their own attributes they want to use
* Actually create releases so the user doesn't have to actually build the MessageSerializer themselves
