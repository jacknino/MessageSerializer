---
layout: defaultWithNavigation
title: Classes
---
## {{ page.title }}

### Member Classes

You can have a member property of your class that has a type that is of another class and it will be serialized as well as long as it inherits from `IMessageSerializable`.

The definition of this member class should follow all the same rules that you would follow for the parent class.

### Inherited Classes

If your class inherits from another class, the properties of the class you inherit from will be included in your message Serialization/Deserialization as long as the class
being inherited from derives from `IMessageSerializable`.

By default, the properties from the class being inherited from will be serialized/deserialized first (i.e. they will be considered the first fields of the message).
However you can set the `PutInheritedPropertiesLast` property of the <makeLink>MessageClass</makeLink> attribute to `true` to indicate that the inherited properties
should actually go at the end of the message.
