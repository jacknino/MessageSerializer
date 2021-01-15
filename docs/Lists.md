---
layout: defaultWithNavigation
title: Lists
---
## {{ page.title }}

Currently the MessageSerializer only supports list types of `List<T>` where `T` is a type that the MessageSerializer knows how to work with.  
It is on the {{ site.data.linkVariables["ToDoList"] }} to support a wider variety of IEnumerable<T> types.

When you have a list in your class, when the type is being serialized each item in the list will be serialized in order starting at index 0.
Similarly, when deserializing a list each item will be deserialized and added to the back of the list in order.  Note that when deserializing
a list, the list must meet one of the following conditions:

* The list must either be the `Data` part of a {{ site.data.linkVariables["Blob"] }} field
* The list must be the variable length field associated with the {{ site.data.linkVariables["Length"] }} field of the message.
* It is on the {{ site.data.linkVariables["ToDoList"] }} to be able to specify the number of items in the list as an attribute if
the number of values will always be constant.

Without meeting one of the above conditions, the MessageSerializer will not know how many items to deserialize into the list.
