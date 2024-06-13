using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Commons.Exceptions;
using Server.EFcore.Contexts;
using Server.EFcore.Models;
using Server.GameSystems.Items;
using ServerShared.Database;
using Shared.Server.GameServer.DTO;

namespace Server.GameSystems.Player.Accounts
{
    public class AccountRepository
    {
        readonly PasswordHasher<object> passwordHasher = new PasswordHasher<object>();

        readonly ILogger logger;
        readonly DbSettings dbSettings;

        public AccountRepository(
            ILogger<AccountRepository> logger,
            DbSettings dbSettings)
        {
            this.logger = logger;
            this.dbSettings = dbSettings;
        }

        public async Task AddItems(Guid accountId, IEnumerable<Item> newItems)
        {
            using (var context = new EFcore.Contexts.GameDbContext(dbSettings.ConnectionString))
            {
                var dbAccount = await context.Accounts
                    .Where(gameAccount => gameAccount.Id == accountId)
                    .Include(e => e.Inventory)
                    .SingleOrDefaultAsync();

                if (dbAccount is null)
                {
                    throw new InvalidKeyException();
                }

                foreach (var newItem in newItems)
                {
                    newItem.Owner = dbAccount;
                    var dbItem = dbAccount.Inventory
                        .Where(i => i.BaseItemCode == newItem.BaseItemCode)
                        .FirstOrDefault();

                    if (dbItem is not null && dbItem.IsStackable())
                    {
                        dbItem.Count += newItem.Count;
                    }
                    else
                    {
                        dbAccount.Inventory.Add(newItem);
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task AddItem(Guid accountId, Item newItem)
        {
            await AddItems(accountId, new List<Item> { newItem });
        }       

        public async Task<List<Item>> GetInventory(Guid accountId)
        {
            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    return await context.Accounts
                        .Where(account => account.Id == accountId)
                        .Include(account => account.Inventory)
                        .Select(account => account.Inventory)
                        .SingleOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Exception : {nameof(AccountRepository)}.{nameof(GetInventory)} : accountId : {accountId} \n" +
                    $"{ex.Message}";
                logger.Log(LogLevel.Warning, errorMessage);
                return default;
            }
        }

        public async Task<GameWorld?> GetGameWorld(Guid worldId)
        {
            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    return await context.GameWorlds
                        .Where(world => world.Id == worldId)
                        .Include(world => world.Achievements)
                        .Include(world => world.Owner)
                        .SingleOrDefaultAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default;
            }
        }

        public async Task<CharacterDataDTO> GetCurrentCharacter(Guid accountId)
        {
            return default;
        }

        public async Task<GameAccount?> GetAccount(Guid accountId)
        {
            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    return await context.Accounts
                        .Where(account => account.Id == accountId)
                        .Include(account => account.GameWorld)
                        .SingleOrDefaultAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default;
            }
        }

        public async Task Achieve(Guid accountId, string achievementCode)
        {
            using (var context = new GameDbContext(dbSettings.ConnectionString))
            {
                var account = await context.Accounts
                    .Where(e => e.Id == accountId)
                    .Include(e => e.GameWorld.Achievements)
                    .SingleOrDefaultAsync();

                account.GameWorld.Achievements.Add(new CompletedAchievement { AchievementCode = achievementCode });
                await context.SaveChangesAsync();
            }
        }        

    }
}
