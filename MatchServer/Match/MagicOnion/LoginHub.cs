using CloudStructures;
using CloudStructures.Structures;
using MagicOnion;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.EFcore.Contexts;
using Server.EFcore.Models;
using ServerShared.Database;
using ServerShared.Magiconion.Converters;
using ServerShared.Redis;
using Shared.Server.GameServer.DTO;
using Shared.Server.GameServer.Interface.Hubs;
using StackExchange.Redis;

namespace Server.MagicOnion.Hubs
{
    public class LoginHub : StreamingHubBase<ILoginHub, ILoginHubReceiver>, ILoginHub
    {
        public class LoginInformation
        {
            public LoginInformation(Guid accountId, Guid loginToken, Action<RedisChannel, RedisValue, LoginInformation> LoginFromOtherDevice)
            {
                this.accountId = accountId;
                this.loginToken = loginToken;
                this.LoginOtherDeviceRedisHandler = (channel, message) => LoginFromOtherDevice(channel, message, this);
            }
            public readonly Guid accountId = Guid.Empty;
            //Login요청시 Guid생성, 다른 Login요청들어오면 LogOut할때 체크
            public readonly Guid loginToken = Guid.Empty;
            public readonly Action<RedisChannel, RedisValue> LoginOtherDeviceRedisHandler;
        }

        public const string onlinePlayerKey = "onlinePlayers";
        public const string onlinePlayerLockKey = "onlinePlayerLock";

        RedisConnection redis;
        RedisLock<Guid> onlinePlayerLock;
        RedisDictionary<Guid, Guid> onlinePlayer; //accountId -> token 

        readonly ISubscriber redisSubscriber;
        readonly ILogger logger;
        readonly DbSettings dbSettings;
        IGroup room;
        readonly PasswordHasher<object> passwordHasher = new PasswordHasher<object>();
        LoginInformation? loginInformation;
        object loginInfoLock = new();
        public LoginHub(RedisSettings redisServer, ILogger<LoginHub> logger, DbSettings dbSettings)
        {
            this.redis = redisServer.Connection;
            this.logger = logger;
            this.dbSettings = dbSettings;
            redisSubscriber = redisServer.Connection.GetConnection().GetSubscriber();
            onlinePlayer = new(this.redis, onlinePlayerKey, null);
            onlinePlayerLock = new(this.redis, onlinePlayerLockKey);
        }

        protected override async ValueTask OnDisconnected()
        {
            await LogOut();
        }

        async Task ILoginHub.ServerConnect()
        {
            this.room = await Group.AddAsync(Guid.NewGuid().ToString());
        }

        public async Task<(Guid token, GameAccountDTO? dto)> Login(string userId, string password)
        {
            if (room is null)
            {
                throw new ReturnStatusException((Grpc.Core.StatusCode)111, "Call ServerConnect First");
            }

            lock (loginInfoLock)
            {
                if (this.loginInformation != null)
                {
                    throw new ReturnStatusException((Grpc.Core.StatusCode)109, "already Login");
                }
            }

            GameAccount? gameAccount = null;
            try
            {
                using (var context = new EFcore.Contexts.GameDbContext(dbSettings.ConnectionString))
                {
                    var account = await context.Accounts
                        .Where(account => account.LoginId == userId)
                        .Include(account => account.CharacterDatas)
                        .Include(account => account.GameWorld)
                        .Include(account => account.Inventory)
                        .SingleOrDefaultAsync();
                    if (account is not null)
                    {
                        var verifyResult = passwordHasher.VerifyHashedPassword(null, account.LoginPassword, password);
                        if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            gameAccount = account;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "loginCheckFail from LoginAsync");
            }

            if (gameAccount is null)
            {
                throw new ReturnStatusException((Grpc.Core.StatusCode)99, "invalid account");
            }


            var loginInformation = new LoginInformation(
                accountId: gameAccount.Id,
                loginToken: Guid.NewGuid(),
                LoginFromOtherDevice);

            lock (loginInfoLock)
            {
                if (this.loginInformation != null)
                {
                    throw new ReturnStatusException((Grpc.Core.StatusCode)109, "already Login");
                }
                this.loginInformation = loginInformation;
            }

            //다른 Login계정 LogOut
            if ((await onlinePlayer.ExistsAsync(loginInformation.accountId)))
            {
                await redisSubscriber.PublishAsync(
                    LoginFromOtherDeviceChannel(loginInformation.accountId),
                    loginInformation.loginToken.ToString());
            }
            await redisSubscriber.SubscribeAsync(
                LoginFromOtherDeviceChannel(loginInformation.accountId),
                loginInformation.LoginOtherDeviceRedisHandler);

            //loginToken Update
            Guid lockToken = Guid.NewGuid();
            try
            {
                if (await onlinePlayerLock.TakeAsync(lockToken, expiry: TimeSpan.FromSeconds(20)))
                {
                    await onlinePlayer.SetAsync(loginInformation.accountId, loginInformation.loginToken);
                }
            }
            catch (Exception e)
            {
                logger.LogInformation(e.Message);
            }
            finally
            {
                await onlinePlayerLock.ReleaseAsync(lockToken);
            }

            return (loginInformation.loginToken, gameAccount.ToGameAccountDTO());
        }

        public async Task LogOut()
        {
            await LogOut(this.loginInformation);
        }

        async Task LogOut(LoginInformation? loginInformation)
        {
            lock (loginInfoLock)
            {
                this.loginInformation = null;
            }

            if (loginInformation is null)
            {
                return;
            }

            await redisSubscriber.UnsubscribeAsync(LoginFromOtherDeviceChannel(loginInformation.accountId), loginInformation.LoginOtherDeviceRedisHandler);

            Guid lockToken = Guid.NewGuid();
            try
            {
                if (await onlinePlayerLock.TakeAsync(lockToken, expiry: TimeSpan.FromSeconds(20)))
                {
                    var redisToken = (await onlinePlayer.GetAsync(loginInformation.accountId)).GetValueOrDefault();
                    if (redisToken == loginInformation.loginToken)
                    {
                        await onlinePlayer.DeleteAsync(loginInformation.accountId);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogInformation(e.Message);
            }
            finally
            {
                await onlinePlayerLock.ReleaseAsync(lockToken);
            }
        }

        public async Task<bool> ChangePlayerName(string name)
        {
            if (loginInformation is null)
            {
                return false;
            }

            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    var account = await context.Accounts
                        .Where(account => account.Id == loginInformation.accountId)
                        .ExecuteUpdateAsync(account => account.SetProperty(e => e.PlayerName, e => name));
                }
                return true;
            }
            catch (Exception e)
            {
                logger.LogWarning(e.Message);
                return false;
            }
        }

        private async void LoginFromOtherDevice(RedisChannel channel, RedisValue message, LoginInformation loginInformation)
        {
            ////다른 Login요청인지 검사
            //if (Guid.Parse((string?)message ?? string.Empty) == loginInformation.loginToken)
            //{
            //    return;
            //}

            try
            {
                if (room is not null)
                {
                    BroadcastToSelf(room)?.LoginFromOtherDevice();
                }
                await LogOut(loginInformation);
            }
            catch (Exception e)
            {
                logger.LogInformation(e.Message);
            }
        }

        public async Task<List<ItemDTOBase>?> GetInventory()
        {
            if (loginInformation is null)
            {
                return default;
            }

            List<Item>? inventory = null;
            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    inventory = await context.Accounts
                        .Where(account => account.Id == loginInformation.accountId)
                        .Include(account => account.Inventory)
                        .Select(account => account.Inventory)
                        .SingleOrDefaultAsync();
                }
                if (inventory is null)
                {
                    return default;
                }
                return inventory.ConvertAll(DTOConvertExtension.ToItemDTOBase);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return default;
            }
        }

        RedisChannel LoginFromOtherDeviceChannel(Guid accountId)
        {
            return RedisChannel.Literal($"login/{accountId.ToString()}");
        }
    }
}
