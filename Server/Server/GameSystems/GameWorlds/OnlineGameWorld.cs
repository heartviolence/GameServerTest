using CloudStructures.Structures;
using ConcurrentCollections;
using Microsoft.Identity.Client;
using Server.EFcore.Models;
using Server.GameSystems.Items;
using Server.GameSystems.Player.Accounts;
using Server.GameSystems.Sheets;
using Shared.Server.Sheets;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.GameSystems.GameWorlds
{
    public class OnlineGameWorld
    {
        #region NestedType
        public class Achiever
        {
            private OnlineGameWorld gameWorld;
            public Achiever(OnlineGameWorld gameWorld)
            {
                this.gameWorld = gameWorld;
            }

            public async Task<bool> AchieveAsync(string achievementCode)
            {
                try
                {
                    await gameWorld.achieveSemaphore.WaitAsync();
                    await gameWorld.accountRepository.Achieve(gameWorld.OwnerId, achievementCode);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    gameWorld.achieveSemaphore?.Release();
                }

                var achievement = gameWorld.sheets.Achievements[achievementCode];
                if (achievement is null)
                {
                    return false;
                }
                var DropItemReward = achievement.Where(e => e.Type == AchievementSheet.RewardType.DropItem);
                var AcquireItemReward = achievement.Where(e => e.Type == AchievementSheet.RewardType.AcquireItem);

                if (DropItemReward.Count() > 0)
                {
                    await ProcessDropRewards(DropItemReward, achievementCode);
                }
                if (AcquireItemReward.Count() > 0)
                {
                    await ProcessAcquireItemRewards(AcquireItemReward, achievementCode);
                }

                return true;
            }

            private async Task ProcessDropRewards(IEnumerable<AchievementSheet.Reward> rewards, string achievementCode)
            {
                var spawnedDropItems = new List<DropItem>();

                foreach (var reward in rewards)
                {
                    if (reward.Count <= 0)
                    {
                        continue;
                    }

                    List<Guid> ownerList;
                    if (reward.RecieveTarget == AchievementSheet.RecieveTarget.Host)
                    {
                        ownerList = new List<Guid> { gameWorld.gameRoom.HostId };
                    }
                    else
                    {
                        ownerList = gameWorld.gameRoom.Guests.ToList();
                        ownerList.Add(gameWorld.gameRoom.HostId);
                    }

                    foreach (var ownerId in ownerList)
                    {
                        var dropitem = new DropItem()
                        {
                            SourceAchievementCode = achievementCode,
                            OwnerId = ownerId,
                            BaseItemCode = reward.TargetCode,
                            Count = reward.Count
                        };
                        spawnedDropItems.Add(dropitem);
                    }
                }

                gameWorld.AddDropItems(spawnedDropItems);
            }

            private async Task ProcessAcquireItemRewards(IEnumerable<AchievementSheet.Reward> rewards, string achievementCode)
            {
                List<Item> items = new List<Item>();
                foreach (var reward in rewards)
                {
                    if (reward.Count <= 0)
                    {
                        continue;
                    }

                    try
                    {
                        var baseItem = gameWorld.sheets.Items[reward.TargetCode];
                        if (baseItem is null)
                        {
                            return;
                        }
                        var newItem = baseItem.ToItem();
                        newItem.Count = reward.Count;
                        items.Add(newItem);
                    }
                    catch { }
                }

                try
                {
                    await gameWorld.itemSemaphore.WaitAsync();
                    await gameWorld.accountRepository.AddItems(gameWorld.OwnerId, items);
                    gameWorld.RewardReceieved?.Invoke(new Rewards()
                    {
                        accountId = gameWorld.OwnerId,
                        items = items,
                    });
                }
                catch
                {
                    return;
                }
                finally
                {
                    gameWorld.itemSemaphore.Release();
                }
            }
        }

        public struct Rewards
        {
            public Guid accountId;
            public List<Item> items;
        }
        #endregion
        public Guid OwnerId { get; }
        private List<DropItem> DropItems { get; } = new();

        public Action<List<DropItem>> DropItemAdded { get; set; }
        public Action<DropItem> DropItemRemoved { get; set; }
        public Action<Rewards> RewardReceieved { get; set; }
        AccountRepository accountRepository { get; set; }
        GameSheetContainer sheets;
        RoomInformation gameRoom;
        Achiever achiever;

        SemaphoreSlim itemSemaphore = new SemaphoreSlim(1, 1); //DB Item Table 건들때 Lock 
        SemaphoreSlim achieveSemaphore = new SemaphoreSlim(1, 1); //DB achievement table 건들때 Lock

        public OnlineGameWorld(Guid ownerId, AccountRepository accountRepository, GameSheetContainer sheets, RoomInformation gameRoom)
        {
            this.OwnerId = ownerId;
            this.accountRepository = accountRepository;
            this.sheets = sheets;
            this.gameRoom = gameRoom;
            achiever = new(this);
        }

        public async Task<bool> AchieveAsync(string achievementCode)
        {
            return await achiever.AchieveAsync(achievementCode);
        }

        public async Task<bool> LootDropItemAsync(Guid accountId, Guid dropItemId)
        {
            DropItem dropItem;
            lock (DropItems)
            {
                dropItem = DropItems.Where(dropItem => dropItem.Id == dropItemId).SingleOrDefault();
            }

            if (dropItem is null)
            {
                return false;
            }
            if (!RemoveDropItem(dropItem))
            {
                return false;
            }

            try
            {
                var newItem = sheets.Items[dropItem.BaseItemCode]?.ToItem();
                if (newItem is not null)
                {
                    newItem.Count = dropItem.Count;

                    try
                    {
                        await itemSemaphore.WaitAsync();
                        await accountRepository.AddItem(accountId, newItem);
                    }
                    finally
                    {
                        itemSemaphore?.Release();
                    }
                }
            }
            catch { }

            return true;
        }

        private void AddDropItems(List<DropItem> items)
        {
            if (items is null || items.Count <= 0)
            {
                return;
            }
            lock (this.DropItems)
            {
                this.DropItems.AddRange(items);
            }
            this.DropItemAdded?.Invoke(items);
        }

        private bool RemoveDropItem(DropItem item)
        {
            bool result = false;
            lock (this.DropItems)
            {
                result = this.DropItems.Remove(item);
            }
            if (result)
            {
                this.DropItemRemoved?.Invoke(item);
            }
            return result;
        }
    }
}
