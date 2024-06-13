
using Cysharp.Threading.Tasks;
using MessagePipe;
using Shared.Server.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

public class InventoryGridPresenter : IAsyncStartable, IDisposable
{
    readonly InventoryGrid ui;
    readonly IAsyncSubscriber<InventoryFilterChangedEvent> filterChanged;
    readonly InventoryManager inventory;
    readonly SheetContainer dataSheets;
    readonly IAsyncPublisher<InventoryGridSelectedItemChanged> selectedItemChange;

    CancellationTokenSource refreshCts = new();

    Func<Item, bool> filter;
    IDisposable subscription;
    InventoryGridItem currentFocused = null;
    public InventoryGridPresenter(
        InventoryGrid ui,
        IAsyncSubscriber<InventoryFilterChangedEvent> filterChanged,
        InventoryManager inventory,
        SheetContainer dataSheets,
        IAsyncPublisher<InventoryGridSelectedItemChanged> selectedItemChange)
    {
        this.ui = ui;
        this.filterChanged = filterChanged;
        this.inventory = inventory;
        this.dataSheets = dataSheets;
        this.selectedItemChange = selectedItemChange;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        var bag = DisposableBag.CreateBuilder();
        filterChanged.Subscribe(async (e, ct) =>
        {
            filter = e.Filter;
            await RefreshUIAsync();
        }).AddTo(bag);
        subscription = bag.Build();
    }

    public async UniTask RefreshUIAsync()
    {
        refreshCts.Cancel();
        refreshCts.Dispose();
        refreshCts = new();
        CancellationToken ct = refreshCts.Token;

        IEnumerable<Item> filteredItems = inventory.items;
        if (filter != null)
        {
            filteredItems = filteredItems.Where(filter);
        }

        currentFocused = null;
        ui.Clear();
        foreach (var item in filteredItems)
        {
            Sprite backgroundImage = null;
            Sprite itemImage = null;
            var baseItem = dataSheets.Items[item.BaseItemCode];
            if (baseItem != null)
            {
                backgroundImage = await SpriteLoadUtil.LoadGridItemRankBackground(baseItem.ItemRank);
            }
            else
            {
                backgroundImage = await SpriteLoadUtil.LoadGridItemRankBackground(ItemSheet.ItemRank.Common);
            }
            itemImage = await SpriteLoadUtil.LoadItemSpriteAsync(item.BaseItemCode);
            if (ct.IsCancellationRequested)
            {
                Addressables.Release(itemImage);
                Addressables.Release(backgroundImage);
                throw new OperationCanceledException();
            }
            var child = ui.NewChild();
            child.OnDispose += () =>
            {
                Addressables.Release(itemImage);
                Addressables.Release(backgroundImage);
            };
            child.ItemImage.sprite = itemImage;
            child.ItemRankBackground.sprite = backgroundImage;
            child.ItemCount.text = item.Count.ToString();

            child.Clicked += (e) =>
            {
                SetFocus(child);
                selectedItemChange.PublishAsync(new InventoryGridSelectedItemChanged()
                {
                    SelectedItem = item
                });
            };
        }
        ui.children.ForEach(child => child.SetActive(true));
    }

    void SetFocus(InventoryGridItem nextFocus)
    {
        if (currentFocused == nextFocus)
        {
            return;
        }
        if (currentFocused != null)
        {
            currentFocused.Focused = false;
        }
        nextFocus.Focused = true;
        currentFocused = nextFocus;
    }

    public void Dispose()
    {
        subscription?.Dispose();
    }
}