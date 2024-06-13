
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

public class ControllerState
{
    public List<IInteractable> interactiveList = new();
    readonly IAsyncPublisher<InteractiveListItemAddedEvent> interactItemAdded;
    readonly IAsyncPublisher<InteractiveListItemRemovedEvent> interactItemRemoved;

    CharacterData characterData;

    public float moveSpeed = 5.0f;

    public ControllerState(
         IAsyncPublisher<InteractiveListItemAddedEvent> interactItemAdded,
         IAsyncPublisher<InteractiveListItemRemovedEvent> interactItemRemoved)
    {
        this.interactItemAdded = interactItemAdded;
        this.interactItemRemoved = interactItemRemoved;
    }

    public void AddInteractiveItem(IInteractable item)
    {
        if (item == null)
        {
            return;
        }
        interactiveList.Add(item);
        interactItemAdded.PublishAsync(new InteractiveListItemAddedEvent()
        {
            item = item
        });
    }

    public void RemoveInteractiveItem(IInteractable item)
    {
        if (interactiveList.Remove(item))
        {
            interactItemRemoved.PublishAsync(new InteractiveListItemRemovedEvent()
            {
                item = item
            });
        }
    }
}