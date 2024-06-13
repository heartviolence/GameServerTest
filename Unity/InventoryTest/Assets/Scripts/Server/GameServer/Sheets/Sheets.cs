using Cathei.BakingSheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Shared.Server.Sheets
{
    public class ItemSheet : Sheet<ItemSheet.Row>
    {
        public enum ItemType
        {
            Equip,
            Consumable
        }

        public enum ItemRank
        {
            Common,
            Rare,
            Unique,
            Legendary
        }

        public class Row : SheetRow
        {
            public string Name { get; private set; }
            public ItemType ItemType { get; private set; }
            public ItemRank ItemRank { get; private set; }
            public float Attack { get; private set; }
            public float Defense { get; private set; }
            public float HP { get; private set; }
            public string AdditionalEffect { get; private set; }
            public string Description { get; private set; }
        }
    }

    public class AchievementSheet : Sheet<AchievementSheet.Row>
    {
        public enum RewardType
        {
            DropItem,
            AcquireItem
        }
        public enum RecieveTarget
        {
            Host,
            AllRoomMember
        }
        public class Reward : SheetRowElem
        {
            public RewardType Type { get; set; }
            public string TargetCode { get; set; }
            public int Count { get; set; }
            public RecieveTarget RecieveTarget { get; set; }
        }

        public class Row : SheetRowArray<Reward>
        {
            public string Name { get; private set; }
            public string InteractCode { get; private set; }
            public string Description { get; private set; }

            public Reward GetReward(int index)
            {
                return this[index];
            }
            public int RewardCount => Count;
        }
    }

    public class InteractiveObjectSheet : Sheet<InteractiveObjectSheet.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
        }
    }
}