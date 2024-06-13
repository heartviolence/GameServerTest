using Cysharp.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MessagePipe;
using Shared.Data;
using Shared.Server.GameServer.DTO;
using Shared.Server.GameServer.Interface.Hubs;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GameWorld : IGameWorldHubRecevier, IDisposable
{
    public class Dependencies
    {
        public Dependencies(
            WorldAchievementManager achievements,
            WorldDropItemManager dropItems,
            IAsyncPublisher<PlayerEnteredEvent> playerAdded,
            IAsyncPublisher<PlayerLeavedEvent> playerLeaved,
            IAsyncPublisher<EnterRequestReceivedEvent> enterRequestRecieved,
            IAsyncPublisher<DropItemAddedEvent> dropItemAdded,
            IAsyncPublisher<DropItemRemovedEvent> dropItemRemoved,
            IAsyncSubscriber<MyCharacterDirectionChangedEvent> myCharacterDirectionChanged,
            IAsyncPublisher<OtherCharacterDirectionChangedEvent> otherCharacterDirectionChanged,
            IAsyncPublisher<GetRewardsEvent> inventoryItemAdded)
        {
            this.achievements = achievements;
            this.dropItems = dropItems;
            this.playerAdded = playerAdded;
            this.playerLeaved = playerLeaved;
            this.enterRequestRecieved = enterRequestRecieved;
            this.dropItemAdded = dropItemAdded;
            this.dropItemRemoved = dropItemRemoved;
            this.myCharacterDirectionChanged = myCharacterDirectionChanged;
            this.otherCharacterDirectionChanged = otherCharacterDirectionChanged;
            this.inventoryItemAdded = inventoryItemAdded;
        }
        readonly public WorldAchievementManager achievements;
        readonly public WorldDropItemManager dropItems;
        readonly public IAsyncPublisher<PlayerEnteredEvent> playerAdded;
        readonly public IAsyncPublisher<PlayerLeavedEvent> playerLeaved;
        readonly public IAsyncPublisher<EnterRequestReceivedEvent> enterRequestRecieved;
        readonly public IAsyncPublisher<DropItemAddedEvent> dropItemAdded;
        readonly public IAsyncPublisher<DropItemRemovedEvent> dropItemRemoved;
        readonly public IAsyncPublisher<OtherCharacterDirectionChangedEvent> otherCharacterDirectionChanged;
        readonly public IAsyncSubscriber<MyCharacterDirectionChangedEvent> myCharacterDirectionChanged;
        readonly public IAsyncPublisher<GetRewardsEvent> inventoryItemAdded;
    }
    ChannelBase channel;
    public IGameWorldHub Hub { get; private set; }
    public Guid WorldId { get; }
    Guid accountId;

    CancellationTokenSource lifeCts = new CancellationTokenSource();

    IDisposable subscription;
    readonly Dependencies dependencies;

    public GameWorld(
        string gameServerAddress,
        Guid worldId,
        Dependencies dependencies)
    {
        this.channel = GrpcChannelx.ForAddress(gameServerAddress);
        this.WorldId = worldId;
        this.dependencies = dependencies;
        var bag = DisposableBag.CreateBuilder();
        dependencies.myCharacterDirectionChanged.Subscribe(async (e, ct) =>
        {
            await CharacterDirectionChanged(e.currentPosition, e.currentDirection);
        }).AddTo(bag);
        subscription = bag.Build();

        dependencies.dropItems.Reset();
        dependencies.achievements.Reset();
        dependencies.dropItems.CurrentWorld = this;
    }

    public async UniTask<bool> LoginAsync(Guid token, Guid accountId)
    {
        bool LoginResult = false;
        try
        {
            this.Hub = await StreamingHubClient.ConnectAsync<IGameWorldHub, IGameWorldHubRecevier>(channel, this);
            if (LoginResult = await this.Hub.LoginCheck(token, accountId))
            {
                this.accountId = accountId;
            }
            Debug.Log($"LoginCheck : {LoginResult}");
        }
        catch (Exception ex)
        {
            Debug.Log("GameWorld Login Failed");
            Debug.LogException(ex);
            return false;
        }
        return LoginResult;
    }

    //Sucess -> return true , Fail-> return false
    public async UniTask<bool> EnterAsync()
    {
        try
        {
            var gameworldData = await this.Hub?.EnterAsync(this.WorldId);
            gameworldData?.Achievements?.ForEach((achievement) =>
            {
                if (!string.IsNullOrEmpty(achievement.AchievementCode))
                {
                    dependencies.achievements.Processed.Add(achievement.AchievementCode);
                }
            });
            return (gameworldData != null);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return false;
    }

    public async UniTask<bool> AchieveAsync(string achievementCode)
    {
        var result = await this.Hub.Achieve(achievementCode);
        return result;
    }

    public async UniTask LootDropItemAsync(Guid itemId)
    {
        await this.Hub.LootDropItem(itemId);
    }

    public async UniTask AcceptGuestAsync(Guid guestId)
    {
        await this.Hub.AcceptGuestAsync(guestId);
    }

    public async UniTask CharacterDirectionChanged(Vector3 currentPosition, Vector3 currentDir)
    {
        await this.Hub.CharacterDirectionChanged(currentPosition, currentDir);
    }

    #region interfaces    

    void IGameWorldHubRecevier.OnGuestEnterRequest(Guid guestId)
    {
        dependencies.enterRequestRecieved.PublishAsync(new()
        {
            guestId = guestId
        }, lifeCts.Token);
    }

    void IGameWorldHubRecevier.OnDropItemAdded(List<DropItemDTO> dropItemDtos)
    {
        Debug.Log("dropItemAdded : " + dropItemDtos.Count);
        try
        {
            var dropItemDatas = (dropItemDtos.ConvertAll(DropItemData.From));
            foreach (var data in dropItemDatas)
            {
                dependencies.dropItems?.AddItem(data);
            }
        }
        catch { }
    }

    void IGameWorldHubRecevier.OnDropItemRemoved(DropItemDTO dto)
    {
        dependencies.dropItems?.RemoveItem(dto.Id);
    }

    void IGameWorldHubRecevier.OnOtherCharacterDirectionChanged(Guid playerId, Vector3 currentPosition, Vector3 currentDir)
    {
        dependencies.otherCharacterDirectionChanged.PublishAsync(new OtherCharacterDirectionChangedEvent()
        {
            accountId = playerId,
            currentPosition = currentPosition,
            currentDirection = currentDir
        }, lifeCts.Token);
    }

    void IGameWorldHubRecevier.OnOtherPlayerEnter(Guid playerId, CharacterDataDTO characterData)
    {
        Debug.Log($"OtherPlayerEnter : {playerId}");
        dependencies.playerAdded.PublishAsync(new PlayerEnteredEvent()
        {
            playerInfo = new(playerId, characterData)
        });
    }

    void IGameWorldHubRecevier.OnOtherPlayerLeave(System.Guid playerId)
    {
        Debug.Log($"OtherPlayerLeave : {playerId}");
        dependencies.playerLeaved.PublishAsync(new PlayerLeavedEvent()
        {
            accountId = playerId
        }, lifeCts.Token);
    }
    void IGameWorldHubRecevier.OnAchievementCompleted(string achievementCode)
    {
        Debug.Log($"AchieveMentCompleted : {achievementCode}");
        dependencies.achievements.Achieve(achievementCode).Forget();
    }
    #endregion

    public async UniTask LoadCharactersAsync()
    {
        try
        {
            var roomMembers = await this.Hub.GetRoomMembers();
            foreach (var roomMember in roomMembers)
            {
                await dependencies.playerAdded.PublishAsync(new PlayerEnteredEvent()
                {
                    playerInfo = roomMember
                });
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void OnRewardsReceived(Guid accountId, List<ItemDTOBase> items)
    {
        if (accountId != this.accountId)
        {
            return;
        }
        UnityEngine.Debug.Log("RewardReceived");

        dependencies.inventoryItemAdded.PublishAsync(new GetRewardsEvent()
        {
            items = items.ConvertAll(Item.From)
        });
    }

    public void Dispose()
    {
        this.lifeCts.Cancel();
        this.lifeCts.Dispose();
        subscription?.Dispose();
        dependencies.dropItems.CurrentWorld = null;
        Hub.DisposeAsync();
    }
}
