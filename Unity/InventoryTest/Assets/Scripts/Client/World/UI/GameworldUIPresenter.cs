
using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;
using VContainer.Unity;

public class GameworldUIPresenter : IAsyncStartable
{
    readonly UINavigator navigator;
    readonly GameWorldMainUIPresenter mainUIPresenter;
    readonly InventoryUIPresenter inventoryUIPresenter;
    readonly MultiplayerListUIPresenter multiplayerListUIPresenter;
    public GameworldUIPresenter(
        UINavigator navigator,
        GameWorldMainUIPresenter mainUIPresenter,
        InventoryUIPresenter inventoryUIPresenter,
        MultiplayerListUIPresenter multiplayerListUIPresenter)
    {
        this.navigator = navigator;
        this.mainUIPresenter = mainUIPresenter;
        this.inventoryUIPresenter = inventoryUIPresenter;
        this.multiplayerListUIPresenter = multiplayerListUIPresenter;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await navigator.StartAsync(cancellation);
        await mainUIPresenter.StartAsync(cancellation);
        await inventoryUIPresenter.StartAsync(cancellation);
        await multiplayerListUIPresenter.StartAsync(cancellation);

        await navigator.GoTo(MainUIState.GameWorld);
    }
}