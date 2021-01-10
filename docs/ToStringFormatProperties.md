---
layout: defaultWithNavigation
title: ToStringFormatProperties
---
## {{ page.title }}

`ToStringFormatProperties` are used to tell the {{ site.data.linkVariables["Serializer"] }} `ToString` method how to format the string.
This includes what string to use to separate each property, what string to use to separate the name of the property from the value of the property
and how things should be indented.

`ToStringFormatProperties` essentially consists of three sets of <makeLink>ToStringDecorationProperties</makeLink>, one each for: `Fields`, `ListItemHeaders` and `ListItems`.
There are also a couple of additional properties related to how lists will be treated.

### Fields

`Fields` should be set to a <makeLink>ToStringDecorationProperties</makeLink> object that indicates how each field should be treated.

`Fields` defaults to the default <makeLink>ToStringDecorationProperties</makeLink>.

### ListItemHeaders

When there is a list of items (e.g. List<SomeClass>) there can be a header for each item in the list such as `Item 0`, `Item 1`, etc.
To have each item in a list have a header, `NumberListItems` should be set to `true` and `ListItemHeaders` should be set 
to a <makeLink>ToStringDecorationProperties</makeLink> object that indicates how the `ListItemHeaders` should be formatted.

`ListItemHeaders` defaults to the default <makeLink>ToStringDecorationProperties</makeLink> except for `Separator` is set to an empty string (i.e. "").

See the note below about why you don't want a `Separator` for `ListItems` but for `ListItemHeaders` you don't really want a separator either
because it will actually go at the end of the previous list item.

### ListItems

`ListItems` are treated much the same as `Fields` but the <makeLink>ToStringDecorationProperties</makeLink> for `ListItems` are
intended to indicate how to handle each list item.

`ListItems` defaults to the default <makeLink>ToStringDecorationProperties</makeLink> except for `Separator` is set to an empty string (i.e. "").

When there is a ListItemHeader you don't really want a ListItems separator because you will get something like:
```
Index 0:
    Value0
Index 1: ,
    Value1
```

That comma at the end of the Index 1 line is actually the separator for Value1 since it's not the 1st item

### NumberListItems

`NumberListItems` is a `bool` value used to indicate whether or not each item is a list should have a 'header' that shows
which item in the list is being shown.

Defaults to `true`.

### ListItemName

If `NumberListItems` is `true` then `ListItemName` is the string that should be used in the list item header.  The header will take the form
of "ListItemName x" where 'x' is the number of the list item.

Defaults to `Index`.

### UseOneBasedListIndex

Normally, the index used in the list item header will start with 0 but if you want them to start with 1, set `UseOneBasedListIndex` to `true`.

Defaults to `false`.

### Default

There is a static `Default` property on `ToStringFormatProperties`.  The `Default` is what is used when the {{ site.data.linkVariables["Serializer"] }} `ToString` method
is called without a `ToStringFormatProperties`.  `Default` is initially set to a new `ToStringFormatProperties` with the defaults for each property as mentioned above.

If you want to use different defaults you can just set `Default` to a different `ToStringFormatProperties` object.

### Extra notes

#### First item indentation

As a general rule the first field of the `ToString` will not be indented so that you can have something like:

```csharp
Console.WriteLine($"My property = {Serializer.Instance.ToString(myProperty)}");
```

and the output will look like:
```
My property = Field1: Value1,
    Field2: Value2,
    Field3: Value3
```

instead of
```
My property =     Field1: Value1,
    Field2: Value2,
    Field3: Value3
```

#### Indenting lists

When you have a list in your class that is define as something like: `List<int> ListPropertyName` you will end up with something like:
```
ListPropertyName: 
    Item 0: 
        10
    Item 1:
        14
```

The indents for Lists can get a little weird because the Fields and ListItems properties can have an effect
when the type of the list is a serializable class.  In this scenario there are a few considerations.
To make things cleaner it was decided that when the fields are going on a separate line
all of the fields should be on the same indent level whereas normally the first field would not be indented
and subsequent fields would.  So you would have something like:

```
ListPropertyName:
    Item 0:
        Field1: 23
        Field2: 32
    Item 1:
        Field1: 44
        Field2: 45
```

instead of:

```
    Item 0:
        Field1: 23
            Field2: 32
    Item 1:
        Field1: 44
            Field2: 45
```

or:

```
    Item 0:
    Field1: 23
        Field2: 32
    Item 1:
    Field1: 44
        Field2: 45
```

Also, there is a question of what if ListItems.Indent = true and Fields.Indent = true
In that scenario you could make an argument that there should then be two indents.
However, things will be limited to one indent and you end up with this matrix:

ListItems.Indent | Fields.Indent | ToString Indented
---              | ---           | ---
true             | true          | yes
false            | true          | yes
true             | false         | yes
false            | false         | no

By "ToString Indented" being yes means it will look like this:
```
    Item 0:
        Field1: 23
        Field2: 32
    Item 1:
        Field1: 44
        Field2: 45
```

"ToString Indented" being no will look like this:
```
    Item 0:
    Field1: 23
    Field2: 32
    Item 1:
    Field1: 44
    Field2: 45
```
