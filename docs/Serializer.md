---
layout: defaultWithNavigation
title: Serializer
---
## {{ page.title }}

The `Serializer` class is the class that is actually used for working with your classes.  This includes the following functionality:

* Taking your class that inherits from `IMessageSerializable` and figuring out using a combination of the {{ site.data.linkVariables["SerializationDefaults"] }},
the attributes on the class or the settings you have specified, how to serialize, deserialize and provide a nicely formatted ToString representation of the values
in your class.  A class is generated in the background that inherits from `SerializerBase` to actually perform these actions.
* Performing the serialization of an object of your class and converting it into an array of bytes
* Deserializing an array of bytes into one of your classes
* Taking an object of your class and creating a ToString for showing the values of your class in a readable way in a format you choose

The `Serializer` class is intended to be used as a singleton through Serializer.Instance but you can inherit from it if you want to have your own version that
perhaps has your defaults set up already or something of that nature.

### Loading Your Class

As a general rule, if you don't need any special handling for your class you don't have to perform the loading of your class information in a separate step.  The first call
to `Serialize`, `Deserialize` or `ToString` will do it for you.  However, it is generally expected that you will load your classes first using the rules you've specified 
in your {{ site.data.linkVariables["SerializationDefaults"] }} and then will actually start processing messages with your class after that.

When loading your classes you can either load them one by one or load them in groups by specifying an assembly or by specifying a list of {{ site.data.linkVariables["ConfigMessageSerializerClass"] }}
objects with information about the classes to load.

To load an individual class you will use one of the `GetClassInfo` or `GetClassInfoFromType` functions.

They have the following signatures:

```csharp
public MessageSerializedClassInfo GetClassInfo(Type type, SerializationDefaults serializationDefaults = null)
public MessageSerializedClassInfo GetClassInfo(Type type, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
public MessageSerializedClassInfo GetClassInfo(Type type, List<ConfigMessageSerializerClass> configMessageSerializerClasses, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
public ConfigMessageSerializerClass CreateClassInfoFromType<T>()
public ConfigMessageSerializerClass CreateClassInfoFromType(Type type, SerializationDefaults serializationDefaults = null)
```

For loading classes as a group you will use either `LoadSerializableClassesFromAssembly` or `LoadSerializableClassesFromSettings`

They have the following signatures:

```csharp
public void LoadSerializableClassesFromAssembly(Assembly assembly)
public void LoadSerializableClassesFromAssembly(Assembly assembly, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
public void LoadSerializableClassesFromSettings(List<ConfigMessageSerializerClass> configMessageSerializerClasses, SerializationDefaults serializationDefaults = null)
public void LoadSerializableClassesFromSettings(List<ConfigMessageSerializerClass> configMessageSerializerClasses, bool replaceIfExists, SerializationDefaults serializationDefaults = null)
```

For either sets of functions the parameters are as follows:
* <T> is the type you want to load the class information from
* `type` is the `Type` that you want to load the class information from
* `assembly` is the `Assembly` that you want to load the class information from
* `serializationDefaults` are the {{ site.data.linkVariables["SerializationDefaults"] }} that you want to use.  When `null` is specified the default instance of `SerializationDefaults` is used (`new SerializationDefaults()`)
* `replaceIfExists` can be used to indicate that even if the class has already been loaded, whatever information has already been loaded should be thrown out and the class should be processed again.
This is mainly intended for unit tests that load the class using attributes and then load the class using the {{ site.data.linkVariables["ConfigMessageSerializerClass"] }} list.
The functions that don't have `replaceIfExists` as a parameter will be treated as if `replaceIfExists` is `false`.
* `configMessageSerializerClasses` are a `List` of {{ site.data.linkVariables["ConfigMessageSerializerClass"] }} that have information about the classes to load.  Note that a list is used
because if the class you want to load contains other classes that need to be serialized they should be included as well.

Remarks:
* When loading classes as a group the functions end up calling `GetClassInfo` for each of the classes in the group
* Calling `CreateClassInfoFromType<T>` calls `CreateClassInfoFromType` with `type` set to `typeof(T)`

### Serializing your class

When you want to take an instance of one of your classes and convert it to an array of bytes, you want to call the `Serialize` method:

```csharp
public byte[] Serialize<T>(T objectToSerialize)
    where T : class, IMessageSerializable
```

As you can see, it is a pretty simple function.  You just pass in the instance of your class and you will get the array of bytes that are created using the rules
that are loaded for your class.

### Deserializing your class

Deserializing is when you want to take an array of bytes and turn them into an instance of your class.  The `Deserialize` and `DeserializeEx` methods are used
for this.  Deserializing is slightly more complicated since the type of the object to return can't be automatically figured out just by the array of bytes so
you have to specify the class you want the class to be deserialized into.

```csharp
public T Deserialize<T>(byte[] bytes) where T : class, IMessageSerializable
public T Deserialize<T>(byte[] bytes, ref int currentArrayIndex) where T : class, IMessageSerializable
public DeserializeResults<T> DeserializeEx<T>(byte[] bytes) where T : class, IMessageSerializable
public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, bool suppressExceptions) where T : class, IMessageSerializable
public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, ref int currentArrayIndex) where T : class, IMessageSerializable
public DeserializeResults<T> DeserializeEx<T>(byte[] bytes, ref int currentArrayIndex, bool suppressExceptions) where T : class, IMessageSerializable
```

The parameters are as follows:
* `<T>` is the type you want to the bytes to be converted to
* `currentArrayIndex` is where in the array of bytes being passed in the deserialization should start.  In most cases you won't need to use this parameter
as it is intended to be used when a class is being deserialized and then that class has a member that also needs to be deserialized.
* `suppressExceptions` can be used to indicate that rather than throwing an exception if a problem occurs during the deserialization the exception should just be include in the `DeserializeResults`.

### ToString

As mentioned previously, when you work with a class with the MessageSerializer a class that inherits from `SerializerBase` is created for your class.  It contains
three methods: `Serialize`, `Deserialize` and `ToString`.  The `ToString` is intended to provide a way for you to show the contents of your class in an easily readable way.
Since the `SerializerBase` class that is created isn't available at design time, the best way to access this `ToString` method is through the `Serializer` class.

```csharp
public string ToString(IMessageSerializable objectToPrint, ToStringFormatProperties formatProperties = null)
public string ToString(IMessageSerializable objectToPrint, int indentLevel, ToStringFormatProperties formatProperties = null)
public string ToString(IMessageSerializable objectToPrint, bool includeBytes, int indentLevel = 0, string separator = null, string bytesSeparator = null, bool putBytesAfter = false, ToStringFormatProperties formatProperties = null)
```

* `objectToPrint` should be self explanatory.  It is the object you want to get the ToString data for.
* `formatProperties` are the <makeLink>ToStringFormatProperties</makeLink> that indicate how you want things formatted (e.g. everything on one line, each property on its own line, indent subclasses, etc.).
If `formatProperties` is `null` then `ToStringFormatProperties.Default` is used.
* `indentLevel` indicates if the string should start out indented to a particular level
* `includeBytes` indicates if the raw bytes should be included in the ToString output as well (the output from `Serialize`)
* `separator` is what string to use to separate the raw bytes of the message from the human readable form (only used if `includeBytes` is `true`)
* `bytesSeparator` is what to use to use to separate individual bytes from each other (only used if `includeBytes` is `true`)
* `putBytesAfter` indicates if the raw bytes should be put before or after the human readable form (only used if `includeBytes` is `true`)

### GetClassInfoString

One other method that the `Serializer` provides is `GetClassInfoString`.  This method can be used to get information about how the MessageSerializer
has determined how to process your class, such as what `TypeSerializer` it is using for each property, what `PropertyRule` was used to determine
how to handle a property and attributes that are on each property.

```csharp
public string GetClassInfoString(Type type, int indentLevel = 0)
public string GetClassInfoString(IMessageSerializable objectForInfo, int indentLevel = 0)
```

* `type` is the `Type` of the class you want to get the info for
* `objectForInfo` is an object of the particular `Type` that you want to get the info for
* `indentLevel` is if you want the info to be indented a certain amount to start
