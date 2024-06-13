
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using VContainer.Unity;

public class InteractiveListPresenter : IAsyncStartable, IDisposable
{
    readonly InteractiveObjectUIList listUI;
    readonly IAsyncSubscriber<InteractiveListItemAddedEvent> itemAdded;
    readonly IAsyncSubscriber<InteractiveListItemRemovedEvent> itemRemoved;
    readonly SheetContainer sheetContainer;
    IDisposable subscriptions;

    CancellationTokenSource lifeCts = new();

    int focusedIndex = -1;
    Dictionary<IInteractable, InteractiveObjectUI> uiDictionary = new();

    public InteractiveListPresenter(
        InteractiveObjectUIList listUI,
        SheetContainer sheetContainer,
        IAsyncSubscriber<InteractiveListItemAddedEvent> itemAdded,
        IAsyncSubscriber<InteractiveListItemRemovedEvent> itemRemoved)
    {
        this.listUI = listUI;
        this.itemAdded = itemAdded;
        this.sheetContainer = sheetContainer;
        this.itemAdded = itemAdded;
        this.itemRemoved = itemRemoved;
    }

    public void Dispose()
    {
        subscriptions?.Dispose();
        lifeCts.Cancel();
        lifeCts?.Dispose();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        var bag = DisposableBag.CreateBuilder();
        itemAdded.Subscribe(async (e, ct) =>
        {
            var child = listUI.NewChild();
            uiDictionary[e.item] = child;

            Sprite itemSprite = null;
            var splits = e.item.InteractCode.Split("-");
            if (splits.Count() > 0)
            {
                if (splits[0] == DataCode.InteractiveObject)
                {
                    itemSprite = await SpriteLoadUtil.LoadInteractiveObject(e.item.InteractCode);
                    child.ItemName = sheetContainer.InteractiveObjects[e.item.InteractCode].Name;
                }
                else if (splits[0] == DataCode.Item)
                {
                    itemSprite = await SpriteLoadUtil.LoadItemSpriteAsync(e.item.InteractCode);
                    child.ItemName = sheetContainer.Items[e.item.InteractCode].Name;
                }
                else
                {
                    child.ItemName = "unknown";
                }
            }
            child.OnDispose += () => Addressables.Release(itemSprite);
            child.ItemSprite = itemSprite;

            if (listUI.children.Count == 1)
            {
                focusedIndex = 0;
                child.Focused = true;
            }
            child.SetActive(true);

            listUI.scrollRect.verticalScrollbar.value = focusedIndex / listUI.children.Count;
        }).AddTo(bag);

        itemRemoved.Subscribe(async (e, ct) =>
        {
            if (uiDictionary.TryGetValue(e.item, out var targetUI))
            {
                int targetIndex = listUI.children.FindIndex(child => child == targetUI);
                bool isFocusedItem = targetUI.Focused;
                listUI.RemoveChild(targetUI);
                if (isFocusedItem)
                {
                    focusedIndex = Mathf.Clamp(targetIndex, -1, listUI.children.Count - 1);
                    if (focusedIndex >= 0)
                    {
                        listUI[focusedIndex].Focused = true;
                    }
                }
                else
                {
                    if (focusedIndex > targetIndex)
                    {
                        focusedIndex -= 1;
                    }
                }

                uiDictionary.Remove(e.item);
                if (listUI.children.Count > 0)
                {
                    listUI.scrollRect.verticalScrollbar.value = focusedIndex / listUI.children.Count;
                }
            }
        }).AddTo(bag);

        subscriptions = bag.Build();

        KeyInput(lifeCts.Token).Forget();
    }

    async UniTask KeyInput(CancellationToken ct)
    {
        while (true)
        {
            await UniTask.NextFrame(ct);

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (focusedIndex >= 0)
                {
                    var pair = uiDictionary.FirstOrDefault(e => e.Value == listUI[focusedIndex]);
                    if (pair.Key != null)
                    {
                        await pair.Key.InteractAsync();
                    }
                    else
                    {
                        Debug.Log("d");
                    }
                }
            }

            if (Input.mouseScrollDelta.y == 1)
            {
                SetFocus(focusedIndex - 1);
            }
            else if (Input.mouseScrollDelta.y == -1)
            {
                SetFocus(focusedIndex + 1);
            }
        }
    }

    void SetFocus(int index)
    {
        if (focusedIndex == index)
        {
            return;
        }

        var minValue = listUI.children.Count > 0 ? 0 : -1;
        var nextFocus = Mathf.Clamp(index, minValue, (listUI.children.Count - 1));

        if (focusedIndex >= 0)
        {
            listUI[focusedIndex].Focused = false;
        }

        if (nextFocus >= 0)
        {
            listUI[nextFocus].Focused = true;
        }
        focusedIndex = nextFocus;
    }

}