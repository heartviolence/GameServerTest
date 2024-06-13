
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class UINavigator : IAsyncStartable, IDisposable
{
    readonly ControlMode controlMode;
    readonly IAsyncPublisher<navigatableUIStateChangedEvent> publisher;

    Stack<MainUIState> beforeStates = new();
    MainUIState currentState = MainUIState.Unknown;
    bool UIChanging = false;

    CancellationTokenSource lifeCts = new();

    public UINavigator(
        ControlMode controlMode,
        IAsyncPublisher<navigatableUIStateChangedEvent> publisher)
    {
        this.controlMode = controlMode;
        this.publisher = publisher;
    }

    public async UniTask GoTo(MainUIState nextState)
    {
        if (nextState == MainUIState.Unknown ||
            nextState == currentState)
        {
            return;
        }

        if (UIChanging)
        {
            return;
        }
        UIChanging = true;
        await ChangeUI(nextState, true);
        UIChanging = false;
    }

    public async UniTask Back()
    {
        if (UIChanging)
        {
            return;
        }
        UIChanging = true;
        if (beforeStates.Peek() != MainUIState.Unknown)
        {
            var beforeUI = beforeStates.Pop();
            await ChangeUI(beforeUI, false);
        }
        UIChanging = false;
    }

    private async UniTask ChangeUI(MainUIState state, bool pushBeforeUI)
    {
        if (state == MainUIState.GameWorld)
        {
            controlMode.SetState(ControlMode.ControlState.Character);
        }
        else
        {
            controlMode.SetState(ControlMode.ControlState.UI);
        }

        if (pushBeforeUI)
        {
            beforeStates.Push(this.currentState);
        }
        this.currentState = state;
        await publisher.PublishAsync(new()
        {
            state = state
        });
        UnityEngine.Debug.Log($"uiNavigated : {state}");
    }

    async UniTask KeyInput(CancellationToken cancellationToken)
    {
        while (true)
        {
            await UniTask.NextFrame(cancellationToken);
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                await Back();
            }
        }
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        KeyInput(lifeCts.Token).Forget();
    }

    public void Dispose()
    {
        lifeCts.Cancel();
        lifeCts.Dispose();
    }
}