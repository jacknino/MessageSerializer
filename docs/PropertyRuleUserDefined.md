---
layout: defaultWithNavigation
title: User Defined PropertyRules
---
## {{ page.title }}

To add your own {{ site.data.linkVariables["PropertyRule"] }} you just need to follow a few steps:

* Create a new class and either inherit from `IPropertyRule` or inherit from an already existing {{ site.data.linkVariables["PropertyRule"] }}.
* Implement the `Check` function.  See the notes on implementing the `Check` function below.
* Add your new {{ site.data.linkVariables["PropertyRule"] }} to the `PropertyRules` of your {{ site.data.linkVariables["SerializationDefaults"] }}
* If you are replacing or overriding one of the defaults rules make sure you remove the old rule from the `PropertyRules`.

### Implementing the `Check` function.

The `Check` function has the following signature:
```csharp
void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
```

You can use the values passed in to the message to decide if there are any changes you need to make based on whatever rule you are trying to implement.
It is generally recommended that you only change or add an attribute if the value hasn't been explicitly specified as a {{ site.data.linkVariables["PropertyRule"] }}
is intended to be used as a smart default related to the specific requirements of your message protocol.

See the information about the parameters to the function to determine what you might want to do:
* {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}
* {{ site.data.linkVariables["SerializationDefaults"] }}
* {{ site.data.linkVariables["MessageClass"] }}

### Example

This example assumes a protocol where most messages have a field at the end of the message that is intended to store the SHA-256 result of all the other bytes in the message.
The {{ site.data.linkVariables["PropertyRule"] }} being created will set up any property named "Hash" to be the <makeLink>CalculatedAuthenticationResult</makeLink>
property and will use <makeLink>CalculatorAuthenticationSha256</makeLink> as the `Calculator`.  Also, since it is known that the length of the SHA-256 result will be 32 bytes
the PropertyRule will also set the `Length` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute to 32 if it hasn't already been set to something.
This makes it possible for the `Hash` property to be used in <makeLink>CalculatedLength</makeLink> calculations.

So to start we need to make our `PropertyRule`:

```csharp
public class PropertyRuleSampleHash : IPropertyRule
{
    public void Check(MessageSerializedPropertyInfo messageSerializedPropertyInfo, SerializationDefaults serializationDefaults, MessageClassAttribute classAttribute)
    {
        if (!messageSerializedPropertyInfo.ContainsAuthenticationAttribute
            && messageSerializedPropertyInfo.PropertyInfo.Name.Equals("Hash", StringComparison.InvariantCultureIgnoreCase))
        {
            messageSerializedPropertyInfo.CalculatedFieldAttributes.Add(new CalculatedAuthenticationResultAttribute(typeof(CalculatorAuthenticationSha256)));

            // Since we know the result of the hash is going to be 32 bytes if the Length property hasn't been set we'll set it.
            if (!messageSerializedPropertyInfo.MessagePropertyAttribute.IsLengthSpecified)
                messageSerializedPropertyInfo.MessagePropertyAttribute.Length = 32;
        }
    }
}
```

The class is pretty straightforward:
* If a property is passed to it that doesn't already have a <makeLink>CalculatedAuthentication</makeLink> or <makeLink>CalculatedAuthenticationResult</makeLink> attribute
and the property is named "Hash" (case-insensitive), then a <makeLink>CalculatedAuthenticationResult</makeLink> attribute with the `Calculator` set to `CalculatorAuthenticationSha256`
will be added to the {{ site.data.linkVariables["MessageSerializedPropertyInfo"] }}.
* Additionally, if the above conditions are met and the `Length` property of the {{ site.data.linkVariables["MessageProperty"] }} attribute hasn't explicitly been set to something
then the `Length` will be set to 32.

Now we need to actually use this `PropertyRule` somehow.  First we need to create a class that is expected to actually use the rule.  We can make a simple test class:

```csharp
public class TestPropertyRuleSampleHash : IMessageSerializable
{
    public byte Length { get; set; }
    public int Int { get; set; }
    public byte[] Hash { get; set; }
}
```

There are a couple things to note about this class:
* The `Length` property will actually be considered to be the <makeLink>CalculatedLengthResult</makeLink> field because of the default <makeLink>PropertyRuleLengthField</makeLink>.
This means that the `Length` property will include the length of all the bytes after it (i.e. the `Int` and `Hash` properties) and will automatically be set by the Serializer.
In this case the `Length` will be 36 (4 bytes for `Int` and 32 bytes for `Hash`).
* Because we are going to add our `PropertyRuleSampleHash` the `Hash` property will automatically be set to the SHA-256 of the bytes in the `Length` and `Int` fields.

So now we can actually do something with our class:
* We need to create a {{ site.data.linkVariables["SerializationDefaults"] }} variable and add our `PropertyRuleSampleHash`
to the list of `PropertyRules` that are part of the {{ site.data.linkVariables["SerializationDefaults"] }}.  Since our new PropertyRule doesn't conflict with any of the default rules we
can just add it to the end of the list and we don't need to modify any of the default rules.  
* Then we need to tell the Serializer to load the class information using our new {{ site.data.linkVariables["SerializationDefaults"] }}.
* Finally we can `Serialize` and `Deserialize` the message

```csharp
SerializationDefaults serializationDefaults = new SerializationDefaults();
serializationDefaults.PropertyRules.Add(new PropertyRuleSampleHash());

Serializer.Instance.GetClassInfo(typeof(TestPropertyRuleSampleHash), serializationDefaults);

TestPropertyRuleSampleHash testMessage = new TestPropertyRuleSampleHash();
testMessage.Int = 0x01020304;

byte[] serialized = Serializer.Instance.Serialize(testMessage);
TestPropertyRuleSampleHash deserialized = Serializer.Instance.Deserialize<TestPropertyRuleSampleHash>(serialized);
```

You can see when `Serialize` was called that only the `Int` property had been set.  The {{ site.data.linkVariables["Serializer"] }} will have automatically
calculated the `Length` and `Hash` values and set them in the `testMessage` variable (as well as the `serialized` bytes having the correct values as well).
