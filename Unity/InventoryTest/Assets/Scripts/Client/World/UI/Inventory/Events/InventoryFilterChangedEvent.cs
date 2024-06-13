
using System;
using System.Collections.Generic;

public struct InventoryFilterChangedEvent
{
    public Func<Item, bool> Filter;
}