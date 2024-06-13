
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldDropItemManager
{
    readonly IAsyncPublisher<DropItemAddedEvent> addEvent;
    readonly IAsyncPublisher<DropItemRemovedEvent> removeEvent;
    public GameWorld CurrentWorld { get; set; }
    public Dictionary<Guid, DropItemInteracter> InteractableItems { get; set; } = new();

    public WorldDropItemManager(
        IAsyncPublisher<DropItemAddedEvent> addEvent,
        IAsyncPublisher<DropItemRemovedEvent> removeEvent)
    {
        this.addEvent = addEvent;
        this.removeEvent = removeEvent;
    }

    public void AddItem(DropItemData data)
    {
        InteractableItems.Add(data.Id, new DropItemInteracter(data, async () =>
        {
            await LootItemAsync(data.Id);
        }));
        addEvent.PublishAsync(new DropItemAddedEvent()
        {
            addedItem = data
        });
    }

    public void RemoveItem(Guid itemId)
    {
        if (InteractableItems.Remove(itemId, out var value))
        {
            removeEvent.PublishAsync(new DropItemRemovedEvent()
            {
                id = itemId,
                removedItem = value
            });
        }
    }

    public async UniTask LootItemAsync(Guid itemId)
    {
        if (CurrentWorld != null)
        {
            await CurrentWorld.LootDropItemAsync(itemId);
        }
    }

    //새 월드 진입시에만 호출
    public void Reset()
    {
        InteractableItems.Clear();
    }
}