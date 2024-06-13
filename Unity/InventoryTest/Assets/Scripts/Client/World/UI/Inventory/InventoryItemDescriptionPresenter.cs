
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

public class InventoryItemDescriptionPresenter : IAsyncStartable
{
    readonly InventoryItemDescription ui;
    readonly IAsyncSubscriber<InventoryGridSelectedItemChanged> subscriber;
    readonly SheetContainer dataSheet;

    public InventoryItemDescriptionPresenter(
        InventoryItemDescription ui,
        IAsyncSubscriber<InventoryGridSelectedItemChanged> subscriber,
        SheetContainer dataSheet)
    {
        this.ui = ui;
        this.subscriber = subscriber;
        this.dataSheet = dataSheet;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        List<Sprite> releaseList = new();
        subscriber.Subscribe(async (e, ct) =>
        {
            releaseList.ForEach(item => Addressables.Release(item));
            releaseList.Clear();
            if (e.SelectedItem == null)
            {
                return;
            }

            var baseItem = dataSheet.Items[e.SelectedItem.BaseItemCode];
            if (baseItem == null)
            {
                return;
            }
            Sprite itemSprite = null;
            Sprite background = null;

            releaseList.Add(itemSprite = await SpriteLoadUtil.LoadItemSpriteAsync(e.SelectedItem.BaseItemCode));
            releaseList.Add(background = await SpriteLoadUtil.LoadItemRankBackground(baseItem.ItemRank));

            ui.ItemSprite.sprite = itemSprite;
            ui.ItemRankBackground.sprite = background;
            ui.ItemName.text = baseItem.Name;
            ui.ItemCount.text = $"x {e.SelectedItem.Count.ToString()}";
            ui.ItemDescription.text = baseItem.Description;
        });
    }
}