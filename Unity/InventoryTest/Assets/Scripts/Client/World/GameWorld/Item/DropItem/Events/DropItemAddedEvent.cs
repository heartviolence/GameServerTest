
using System;

public struct DropItemAddedEvent
{
    public DropItemData addedItem;
}
public struct DropItemRemovedEvent
{
    public Guid id;
    public DropItemInteracter removedItem;
}