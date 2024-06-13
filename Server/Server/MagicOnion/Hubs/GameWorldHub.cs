using CloudStructures;
using CloudStructures.Structures;
using MagicOnion.Server.Hubs;
using MessagePipe;
using Microsoft.Extensions.Azure;
using Server.GameSystems;
using Server.GameSystems.GameWorlds;
using Server.GameSystems.Items;
using Server.GameSystems.Player.Accounts;
using Server.GameSystems.Sheets;
using ServerShared.Magiconion.Converters;
using ServerShared.Redis;
using Shared.Server.GameServer.DTO;
using Shared.Server.GameServer.Interface.Hubs;
using StackExchange.Redis;
using UnityEngine;


namespace Server.MagicOnion.Hubs
{
    public class GameWorldHub : StreamingHubBase<IGameWorldHub, IGameWorldHubRecevier>, IGameWorldHub
    {
        #region NestedType
        public enum PlayerMode
        {
            Host,
            Guest,
            Unknown
        }

        public struct LootItemEvent
        {
            public Guid HostAccountId;
            public Guid LootAccountId;
            public Guid DropItemId;
        }

        #endregion

        #region Fields&Constructor

        Guid worldId = Guid.Empty;
        Guid accountId = Guid.Empty;
        private string RoomName { get => this.worldId.ToString(); }
        private string AuthorizedGuestRedisKey { get => RedisChannel.Literal($"{this.worldId.ToString()}/AuthorizedGuests"); }

        IGroup? broadcastRoom;
        AccountRepository accountRepository;
        PlayerMode playerMode = PlayerMode.Unknown;
        RedisConnection redis;
        OnlineGameWorld? hostGameWorld;

        GameSheetContainer sheets;
        RoomInformation gameRoom;

        RedisDictionary<Guid, Guid> onlinePlayer; // accountId(host)
        RedisHashSet<Guid> authorizedGuests; //GuestId

        readonly RoomInformations roomInformations;
        readonly IAsyncPublisher<MultiplayEnterRequestHub.EnterRequestAcceptedEvent> enterRequestAcceptedPublisher;
        readonly IAsyncSubscriber<MultiplayEnterRequestHub.EnterRequestEvent> enterRequestSubscriber;
        readonly IAsyncPublisher<LootItemEvent> lootItemPublisher;
        readonly IAsyncSubscriber<LootItemEvent> lootItemSubscriber;

        ILogger logger;
        IDisposable subscription;

        public GameWorldHub(AccountRepository accountRepository,
            RedisSettings redisSettings,
            GameSheets sheets,
            RoomInformations roomInformations,
            ILogger<GameWorldHub> logger,
            IAsyncPublisher<MultiplayEnterRequestHub.EnterRequestAcceptedEvent> enterRequestAcceptedPublisher,
            IAsyncSubscriber<MultiplayEnterRequestHub.EnterRequestEvent> enterRequestSubscriber,
            IAsyncPublisher<LootItemEvent> lootItemPublisher,
            IAsyncSubscriber<LootItemEvent> lootItemSubscriber)
        {
            this.accountRepository = accountRepository;
            this.redis = redisSettings.Connection;
            this.logger = logger;

            onlinePlayer = new(this.redis, "onlinePlayers", null);
            this.sheets = sheets.Sheets;

            this.enterRequestAcceptedPublisher = enterRequestAcceptedPublisher;
            this.enterRequestSubscriber = enterRequestSubscriber;
            this.lootItemPublisher = lootItemPublisher;
            this.lootItemSubscriber = lootItemSubscriber;
            this.roomInformations = roomInformations;
        }
        #endregion

        #region Interfaces       

        public async Task<GameWorldDataDTO> EnterAsync(Guid worldId)
        {
            if (this.accountId == Guid.Empty)
            {
                return default;
            }
            var gameworld = await accountRepository.GetGameWorld(worldId);
            if (gameworld is null)
            {
                return default;
            }

            this.worldId = worldId;
            this.authorizedGuests = new(this.redis, AuthorizedGuestRedisKey, TimeSpan.FromSeconds(60));

            //호스트라면
            if (gameworld?.Owner.Id == this.accountId)
            {
                this.playerMode = PlayerMode.Host;
                this.broadcastRoom = await Group.AddAsync(RoomName);
                this.gameRoom = new RoomInformation(this.accountId);
                this.roomInformations.TryAddRoom(this.worldId, this.gameRoom);

                this.hostGameWorld = new OnlineGameWorld(
                    this.accountId,
                    this.accountRepository,
                    this.sheets,
                    this.gameRoom);
                this.hostGameWorld.DropItemAdded += OnDropItemAdded;
                this.hostGameWorld.DropItemRemoved += OnDropItemRemoved;
                this.hostGameWorld.RewardReceieved += OnRewardsReceived;
                HostSubscribeEvents();
            }
            else //게스트라면
            {
                playerMode = PlayerMode.Guest;
                bool enterResult = false;
                try
                {
                    if (await authorizedGuests.ContainsAsync(this.accountId))
                    {
                        if (roomInformations.Dictionary.TryGetValue(this.worldId, out this.gameRoom))
                        {
                            if (gameRoom.Guests.Add(this.accountId))
                            {
                                this.broadcastRoom = await Group.AddAsync(RoomName);

                                enterResult = true;
                                Broadcast(broadcastRoom).OnOtherPlayerEnter(this.accountId, null);
                            }
                        }
                    }
                }
                catch { }

                //입장실패시
                if (!enterResult)
                {
                    this.worldId = Guid.Empty;
                    return default;
                }
            }

            return gameworld.ToGameWorldDataDTO();
        }
        //Host용
        public async ValueTask AcceptGuestAsync(Guid guestId)
        {
            if (playerMode == PlayerMode.Guest)
            {
                return;
            }
            await authorizedGuests.AddAsync(guestId);

            enterRequestAcceptedPublisher.Publish(new()
            {
                HostAccountId = this.accountId,
                AcceptedPlayerId = guestId
            });
        }

        public async Task<bool> Achieve(string achievementCode)
        {
            logger.LogInformation(" CompletedAchievement:" + achievementCode);
            if (playerMode == PlayerMode.Host)
            {
                try
                {
                    if (await this.hostGameWorld.AchieveAsync(achievementCode))
                    {
                        Broadcast(this.broadcastRoom).OnAchievementCompleted(achievementCode);
                        return true;
                    }
                }
                catch { return false; }
            }
            return false;
        }

        public async Task LootDropItem(Guid itemId)
        {
            if (playerMode == PlayerMode.Host)
            {
                await this.hostGameWorld.LootDropItemAsync(this.accountId, itemId);
            }
            else if (playerMode == PlayerMode.Guest)
            {
                lootItemPublisher.Publish(new LootItemEvent()
                {
                    HostAccountId = this.worldId,
                    LootAccountId = this.accountId,
                    DropItemId = itemId
                });
            }
        }

        public async Task CharacterDirectionChanged(Vector3 currentPosition, Vector3 currentDir)
        {
            if (this.broadcastRoom is not null)
            {
                BroadcastExceptSelf(this.broadcastRoom).OnOtherCharacterDirectionChanged(this.accountId, currentPosition, currentDir);
            }
        }

        public async Task<List<(Guid, CharacterDataDTO)>> GetRoomMembers()
        {
            if (this.worldId == Guid.Empty)
            {
                return default;
            }

            var result = new List<(Guid, CharacterDataDTO)>();
            var members = this.gameRoom.Guests.ToList();
            members.Add(this.gameRoom.HostId);

            try
            {
                foreach (var member in members)
                {
                    result.Add((member, await accountRepository.GetCurrentCharacter(member)));
                }

                return result;
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> LoginCheck(Guid token, Guid accountId)
        {
            if ((await onlinePlayer.GetAsync(accountId)).GetValueOrDefault() == token)
            {
                this.accountId = accountId;
                return true;
            }
            return false;
        }
        #endregion

        #region Overrides
        protected override async ValueTask OnDisconnected()
        {
            if (this.gameRoom is not null)
            {
                this.gameRoom.Guests.TryRemove(this.accountId);
                if (playerMode == PlayerMode.Host)
                {
                    roomInformations.TryRemoveRoom(this.worldId, out _);
                }
            }

            if (this.broadcastRoom is not null)
            {
                BroadcastExceptSelf(this.broadcastRoom)?.OnOtherPlayerLeave(this.accountId);
            }
            subscription?.Dispose();
        }
        #endregion

        #region Helpers

        void OnDropItemAdded(List<DropItem> dropItems)
        {
            logger.LogInformation("DropItem added : " + dropItems.Count);
            var dto = dropItems.ConvertAll(ConvertExtension.ToDropItemDTO);
            Broadcast(this.broadcastRoom)?.OnDropItemAdded(dto);
        }

        void OnDropItemRemoved(DropItem dropItem)
        {
            logger.LogInformation("DropItem removed : " + dropItem.BaseItemCode);
            var dto = dropItem.ToDropItemDTO();
            Broadcast(this.broadcastRoom)?.OnDropItemRemoved(dto);
        }

        void OnRewardsReceived(OnlineGameWorld.Rewards reward)
        {
            logger.LogInformation($"RewardReceieved : ID_[{reward.accountId}] ");
            Broadcast(this.broadcastRoom)?.OnRewardsReceived(reward.accountId, reward.items.ConvertAll(DTOConvertExtension.ToItemDTOBase));
        }

        private void HostSubscribeEvents()
        {
            var bag = DisposableBag.CreateBuilder();
            enterRequestSubscriber.Subscribe(async (e, ct) =>
            {
                if (e.HostAccountId == this.accountId)
                {
                    BroadcastToSelf(this.broadcastRoom).OnGuestEnterRequest(e.GuestId);
                }
            }).AddTo(bag);

            lootItemSubscriber.Subscribe(async (e, ct) =>
            {
                if (e.HostAccountId == this.accountId)
                {
                    this.hostGameWorld.LootDropItemAsync(e.LootAccountId, e.DropItemId);
                }
            }).AddTo(bag);

            subscription = bag.Build();
        }
        #endregion

    }
}
