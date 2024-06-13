
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer.Unity;

public class GameInitializeSettings
{
    readonly LoginManager loginManager;
    readonly SheetContainer container;

    public bool Initialized { get; private set; } = false;

    public GameInitializeSettings(
        LoginManager loginManager,
        SheetContainer container)
    {
        this.loginManager = loginManager;
        this.container = container;
    }

    public async UniTask InitializeAsync(CancellationToken cancellation)
    {
        Application.targetFrameRate = 120;
        try
        {
            await container.StartAsync(cancellation);
            await Addressables.LoadAssetsAsync<Sprite>(key: "UISprite", callback: null);
            await loginManager.StartAsync(cancellation);
            Debug.Log("GameInitialize Done");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log("GameInitialize failed");
        }
        Initialized = true;
    }
}