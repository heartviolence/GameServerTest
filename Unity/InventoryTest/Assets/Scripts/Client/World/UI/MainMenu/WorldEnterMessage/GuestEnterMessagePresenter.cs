
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class GuestEnterMessagePresenter : IAsyncStartable, IDisposable
{
    public readonly float PopupDuration = 4.0f;

    GuestEnterMessage ui;
    GameWorldManager worldManager;
    IAsyncSubscriber<EnterRequestReceivedEvent> subscriber;
    IDisposable subscription;
    Queue<Guid> enterMessages = new Queue<Guid>();

    bool canAccept = false;
    CancellationTokenSource Popupcts = new();
    CancellationTokenSource lifeCts = new();

    public GuestEnterMessagePresenter(
        GuestEnterMessage ui,
        GameWorldManager worldManager,
        IAsyncSubscriber<EnterRequestReceivedEvent> subscriber)
    {
        this.ui = ui;
        this.subscriber = subscriber;
        this.worldManager = worldManager;
    }


    private async UniTask ProcessKeyInput(CancellationToken ct)
    {
        while (true)
        {
            await UniTask.NextFrame(ct);
            if (Input.GetKeyDown(KeyCode.Y))
            {
                if (canAccept)
                {
                    canAccept = false;
                    var guestId = enterMessages.Peek();
                    Popupcts.Cancel();
                    worldManager.CurrentWorld.AcceptGuestAsync(guestId).Forget();
                }
            }
        }
    }

    private async UniTask ProcessEnterMessage(CancellationToken ct)
    {
        while (true)
        {
            await UniTask.NextFrame(ct);
            Popupcts.Dispose();
            Popupcts = new();
            if (enterMessages.Count > 0)
            {
                canAccept = true;
                await ui.EnableAnimation();
                await UniTask.WaitForSeconds(PopupDuration, cancellationToken: ct);
                await ui.DisableAnimation();

                enterMessages.Dequeue();
                canAccept = false;
            }
        }
    }

    public void Dispose()
    {
        subscription?.Dispose();
        Popupcts.Cancel();
        Popupcts.Dispose();
        lifeCts.Cancel();
        lifeCts.Dispose();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        ProcessEnterMessage(lifeCts.Token).Forget();
        ProcessKeyInput(lifeCts.Token).Forget();

        var bag = DisposableBag.CreateBuilder();
        subscriber.Subscribe(async (e, ct) =>
        {
            enterMessages.Enqueue(e.guestId);
        }).AddTo(bag);
        subscription = bag.Build();
    }
}