
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class InventoryManager
{
    public List<Item> items = new();
    public InventoryManager()
    {

    }

    public void InventoryUpdate(List<Item> items)
    {
        this.items = items;
        this.items.Sort((x, y) => x.BaseItemCode.CompareTo(y.BaseItemCode));
    }
}