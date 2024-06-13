
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Diagnostics;
using System.Threading;

public abstract class NavigatableUIPresenterBase : IDisposable
{
    MainUIState myUIState;
    readonly IAsyncSubscriber<navigatableUIStateChangedEvent> uiChangedSubscriber;
    readonly IDisposable uistateSubscription;
    bool enabled = false;
    CancellationTokenSource lifeCts = null;

    protected CancellationToken ct
    {
        get
        {
            if (lifeCts == null)
            {
                lifeCts = new();
            }
            return lifeCts.Token;
        }
    }

    public NavigatableUIPresenterBase(IAsyncSubscriber<navigatableUIStateChangedEvent> uiChangedSubscriber,
        MainUIState myUIState)
    {
        this.uiChangedSubscriber = uiChangedSubscriber;
        this.myUIState = myUIState;
        var bag = DisposableBag.CreateBuilder();
        this.uiChangedSubscriber.Subscribe(async (e, ct) =>
        {
            if (this.myUIState == e.state && enabled == false)
            {
                await EnableAsync();
                enabled = true;
            }
            else if (this.myUIState != e.state && enabled == true)
            {
                await DisableAsync();
                enabled = false;
            }
        }).AddTo(bag);
        uistateSubscription = bag.Build();
    }

    public abstract UniTask DisableAsync();
    public abstract UniTask EnableAsync();

    public virtual void Dispose()
    {
        uistateSubscription?.Dispose();
        lifeCts?.Cancel();
        lifeCts?.Dispose();
    }
}