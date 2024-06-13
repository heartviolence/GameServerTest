
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MessagePipe;
using Shared.Server.GameServer.DTO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using VContainer.Unity;

public class MultiplayerListUIPresenter : NavigatableUIPresenterBase, IAsyncStartable
{
    readonly MultiplayerList ui;
    readonly UINavigator nav;
    readonly RoomInfoUIListPresenter roomInfoUIListPresenter;

    CancellationTokenSource roomUpdatects;

    public MultiplayerListUIPresenter(
        MultiplayerList ui,
        UINavigator nav,
        RoomInfoUIListPresenter roomInfoUIListPresenter,
        IAsyncSubscriber<navigatableUIStateChangedEvent> uiChangeSubscriber) : base(uiChangeSubscriber, MainUIState.Multiplay)
    {
        this.ui = ui;
        this.nav = nav;
        this.roomInfoUIListPresenter = roomInfoUIListPresenter;
    }

    public override async UniTask DisableAsync()
    {
        roomUpdatects.Cancel();
        roomUpdatects.Dispose();
        roomUpdatects = null;
        await ui.disableAnimation.StartAsync();
    }

    public override async UniTask EnableAsync()
    {
        roomUpdatects = new();
        await roomInfoUIListPresenter.Refresh(roomUpdatects.Token);
        await ui.enableAnimation.StartAsync();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        ui.closeButton.OnClickAsAsyncEnumerable().ForEachAwaitAsync(async (_) =>
        {
            await nav.Back();
        }, ct).Forget();
    }
}