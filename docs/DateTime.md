---
layout: defaultWithNavigation
title: Date/Time
---

DateTime types are serialized based on 3 parameters:
* Format (defaults to `MMddyyyyHHmmss`) and conforms to the ToString specification for DateTime types
* IsBcd (defaults to true for DateTime types) determines if the resulting value is serialized as BCD or as a number.
Currently only IsBcd = true is supported
* Length (defaults to the length of Format / 2) determines the final length of the value.  Currently only the default is supported.

