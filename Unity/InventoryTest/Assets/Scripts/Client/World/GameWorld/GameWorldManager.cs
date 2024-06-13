
using Cysharp.Threading.Tasks;
using ServerShared.Match;
using MessagePipe;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class GameWorldManager : IDisposable
{
    public GameWorld CurrentWorld { get; private set; }
    private LoginManager accountManager;

    readonly MatchServerService matchService;
    readonly IAsyncSubscriber<EnterRequestAcceptedEvent> enterRequestAccepted;
    readonly IAsyncSubscriber<LogoutEvent> logoutsubscriber;
    readonly GameWorld.Dependencies gameWorldDependencies;
    bool isLoading = false;
    IDisposable subscription;
    public GameWorldManager(
        LoginManager accountManager,
        MatchServerService matchService,
        IAsyncSubscriber<EnterRequestAcceptedEvent> enterRequestAccepted,
        IAsyncSubscriber<LogoutEvent> logoutsubscriber,
        GameWorld.Dependencies gameWorldDependencies)
    {
        this.accountManager = accountManager;
        this.matchService = matchService;
        this.enterRequestAccepted = enterRequestAccepted;
        this.logoutsubscriber = logoutsubscriber;
        this.gameWorldDependencies = gameWorldDependencies;
        var bag = DisposableBag.CreateBuilder();
        enterRequestAccepted.Subscribe(async (e, ct) =>
        {
            await LoadWorldAsync(e.serverAddress, e.hostId);
        }).AddTo(bag);
        logoutsubscriber.Subscribe(async (e, ct) =>
        {
            await SceneManager.LoadSceneAsync("LoginScene", LoadSceneMode.Single);
            Cursor.lockState = CursorLockMode.Confined;
        }).AddTo(bag);
        subscription = bag.Build();
    }

    public async UniTask LoadMyWorldAsync()
    {
        var serverList = (await matchService.GetAvailableServerList())
            .OrderByDescending((e) => e.capacity);

        Debug.Log("Server Count :" + serverList.Count());
        foreach (var server in serverList)
        {
            Debug.Log("server: " + server.address);
        }

        foreach (var server in serverList)
        {
            if (await LoadWorldAsync(server.address, accountManager.WorldId))
            {
                return;
            }
        }
    }

    public async UniTask<bool> LoadWorldAsync(string gameServerAddress, Guid worldId)
    {
        if (isLoading)
        {
            return false;
        }
        isLoading = true;

        try
        {
            if (this.CurrentWorld != null)
            {
                this.CurrentWorld.Dispose();
            }
            this.CurrentWorld = new GameWorld(gameServerAddress, worldId, gameWorldDependencies);

            for (var retryCount = 0; retryCount < 5; retryCount++)
            {
                if (!(await this.CurrentWorld.LoginAsync(accountManager.LoginToken, accountManager.AccountId)))
                {
                    continue;
                }
                if (await this.CurrentWorld.EnterAsync())
                {
                    await SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
                    Debug.Log("GameWorld LoadSuccessed");
                    await this.CurrentWorld.LoadCharactersAsync();
                    isLoading = false;
                    return true;
                }
                else
                {
                    Debug.Log($"GameWorld LoadFailed RetryCount : {retryCount + 1}");
                    await UniTask.WaitForSeconds(1);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }

        Debug.Log("GameWorld LoadFailed");
        await SceneManager.LoadSceneAsync("LoginScene", LoadSceneMode.Single);
        isLoading = false;
        return false;
    }

    public void Dispose()
    {
        subscription?.Dispose();
        this.CurrentWorld?.Dispose();
    }
}