
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class MainMenuBarPresenter : IAsyncStartable, IDisposable
{
    readonly MainMenuBar bar;
    readonly UINavigator navigator;
    readonly LoginManager loginManager;

    CancellationTokenSource lifeCts = new();

    public MainMenuBarPresenter(
        MainMenuBar bar,
        UINavigator navigator,
        LoginManager loginManager)
    {
        this.bar = bar;
        this.navigator = navigator;
        this.loginManager = loginManager;
    }

    public async UniTask DisableAsync()
    {
        await bar.DisableAnimation();
    }

    public void Dispose()
    {
        lifeCts.Cancel();
        lifeCts.Dispose();
    }

    public async UniTask EnableAsync()
    {
        await bar.EnableAnimation();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        bar.inventoryIcon.Clicked += (e) =>
        {
            navigator.GoTo(MainUIState.Inventory).Forget();
        };

        bar.MultiplayIcon.Clicked += (e) =>
        {
            navigator.GoTo(MainUIState.Multiplay).Forget();
        };

        bar.ChangeNameIcon.Clicked += (e) =>
        {
            bar.nameChangeField.EnableAnimation().Forget();
        };

        bar.nameChangeField.inputField.onEndEdit.AddListener((text) =>
        {
            UniTask.Action(async () =>
            {
                var result = await loginManager.PlayerNameChange(text);
                Debug.Log($"try nameChange :{text} ,result :{result}");
                await UniTask.NextFrame();
                await bar.nameChangeField.DisableAnimation();
            }).Invoke();
        });

        KeyInput(lifeCts.Token).Forget();
    }

    async UniTask KeyInput(CancellationToken cancellationToken)
    {
        while (true)
        {
            await UniTask.NextFrame(cancellationToken);
            if (Input.GetKeyDown(KeyCode.P))
            {
                navigator.GoTo(MainUIState.Multiplay).Forget();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                navigator.GoTo(MainUIState.Inventory).Forget();
            }
        }
    }
}