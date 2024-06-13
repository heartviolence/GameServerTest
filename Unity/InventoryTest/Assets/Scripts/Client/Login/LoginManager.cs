
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MessagePipe;
using Shared.Data;
using Shared.Server.GameServer.Interface.Hubs;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer.Unity;

public class LoginManager : ILoginHubReceiver, IAsyncStartable, IDisposable
{
    public Guid AccountId { get; private set; }
    public Guid LoginToken { get; private set; }
    public int UID { get; private set; }
    public Guid WorldId { get; private set; }

    public List<string> AchievementCodes { get; set; } = new();
    public List<CharacterData> CharacterDatas { get; set; } = new();

    CancellationTokenSource lifeCts = new CancellationTokenSource();
    ChannelBase channel;
    ILoginHub client;

    readonly IAsyncPublisher<LogoutEvent> logoutPublisher;
    readonly IAsyncPublisher<LoginSuccessedEvent> loginSuccessed;
    readonly IAsyncPublisher<LoginFailedEvent> loginFailed;
    readonly InventoryManager inventoryManager;

    public LoginManager(
        InventoryManager inventoryManager,
        IAsyncPublisher<LogoutEvent> logoutPublisher,
        IAsyncPublisher<LoginSuccessedEvent> loginSuccessed,
        IAsyncPublisher<LoginFailedEvent> loginFailed)
    {
        this.inventoryManager = inventoryManager;
        this.logoutPublisher = logoutPublisher;
        this.loginSuccessed = loginSuccessed;
        this.loginFailed = loginFailed;
    }

    public async UniTask LoginAsync(string userId, string password)
    {
        try
        {
            var result = await client.Login(userId, password);
            var accountDTO = result.dto;
            if (accountDTO == null)
            {
                await loginFailed.PublishAsync(new LoginFailedEvent(), lifeCts.Token);
                return;
            }
            LoginToken = result.token;
            AccountId = accountDTO.AccountId;
            UID = accountDTO.UID;
            WorldId = accountDTO.GameWorld.Id;
            inventoryManager.InventoryUpdate(accountDTO.Inventory.ConvertAll(e => Item.From(e)));
            AchievementCodes = accountDTO.GameWorld.Achievements.ConvertAll(e => e.AchievementCode);
            CharacterDatas = accountDTO.CharacterDatas.ConvertAll(e => CharacterData.From(e));
            await loginSuccessed.PublishAsync(new LoginSuccessedEvent(), lifeCts.Token);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            await loginFailed.PublishAsync(new LoginFailedEvent(), lifeCts.Token);
            return;
        }
    }

    public async UniTask InventoryUpdate()
    {
        var inventory = await this.client.GetInventory();
        if (inventory != null)
        {
            inventoryManager.InventoryUpdate(inventory.ConvertAll(e => Item.From(e)));
        }
    }

    public async UniTask<bool> PlayerNameChange(string name)
    {
        return await this.client.ChangePlayerName(name);
    }

    public async UniTask LogOutAsync()
    {
        AccountId = default;
        LoginToken = default;
        UID = default;
        WorldId = default;
        AchievementCodes.Clear();
        await client.LogOut();
        await logoutPublisher.PublishAsync(new LogoutEvent());
    }

    public void LoginFromOtherDevice()
    {
        Debug.Log("otherDevice Login");
        LogOutAsync().Forget();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        Debug.Log($"Connect to loginServer... :{ServerAddresses.LoginAndMatchServer}");
        channel = GrpcChannelx.ForAddress(ServerAddresses.LoginAndMatchServer);
        client = await StreamingHubClient.ConnectAsync<ILoginHub, ILoginHubReceiver>(channel, this);
        await client.ServerConnect();
        Debug.Log("loginServerConnected");
    }

    public void Dispose()
    {
        lifeCts.Cancel();
        lifeCts.Dispose();
        client?.DisposeAsync();
    }
}
