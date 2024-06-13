
using Cysharp.Threading.Tasks;
using ServerShared.Match;
using MessagePipe;
using Shared.Server.Sheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting.Dependencies.NCalc;
using VContainer.Unity;

public class InventoryTopPresenter : IAsyncStartable
{
    readonly InventoryTopUI ui;
    readonly UINavigator nav;
    readonly IAsyncPublisher<InventoryFilterChangedEvent> publisher;
    readonly SheetContainer sheets;

    Func<Item, bool> equipCategoryFilter;
    Func<Item, bool> ConsumableCategoryFilter;

    InventoryCategoryUI currentFocused = null;

    public InventoryTopPresenter(
        InventoryTopUI ui,
        UINavigator nav,
        SheetContainer sheets,
        IAsyncPublisher<InventoryFilterChangedEvent> publisher)
    {
        this.ui = ui;
        this.nav = nav;
        this.publisher = publisher;
        this.sheets = sheets;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        equipCategoryFilter = (item) =>
        {
            return sheets.Items[item.BaseItemCode]?.ItemType == ItemSheet.ItemType.Equip;
        };

        ConsumableCategoryFilter = (item) =>
        {
            return sheets.Items[item.BaseItemCode]?.ItemType == ItemSheet.ItemType.Consumable;
        };

        ui.closeButton.onClick.AddListener(() =>
        {
            nav.Back().Forget();
        });

        Action<InventoryCategoryUI, InventoryFilterChangedEvent> FilterChange = (ui, e) =>
        {
            if (currentFocused == ui)
            {
                return;
            }
            if (currentFocused != null)
            {
                currentFocused.Focused = false;
            }
            ui.Focused = true;
            currentFocused = ui;
            publisher.PublishAsync(e, default);
        };

        ui.equipCategory.Clicked = (e) =>
        {
            FilterChange(ui.equipCategory,
                new InventoryFilterChangedEvent
                {
                    Filter = equipCategoryFilter
                });
        };

        ui.consumableCategory.Clicked = (item) =>
        {
            FilterChange(
                ui.consumableCategory,
                new InventoryFilterChangedEvent
                {
                    Filter = ConsumableCategoryFilter
                });
        };

        FilterChange(ui.equipCategory,
                new InventoryFilterChangedEvent
                {
                    Filter = equipCategoryFilter
                });
    }
}