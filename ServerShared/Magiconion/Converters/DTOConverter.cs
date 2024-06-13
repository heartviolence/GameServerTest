using Server.EFcore.Models;
using Shared.Server.GameServer.DTO;
using System.Runtime.InteropServices;

namespace ServerShared.Magiconion.Converters
{
    public static class DTOConvertExtension
    {
        public static GameAccountDTO ToGameAccountDTO(this GameAccount source)
        {
            return new GameAccountDTO()
            {
                AccountId = source.Id,
                UID = source.UID,
                Inventory = source.Inventory.ConvertAll(ToItemDTOBase),
                GameWorld = ToGameWorldDataDTO(source.GameWorld),
                CharacterDatas = source.CharacterDatas.ConvertAll(ToCharacterDataDTO)
            };
        }

        static public CharacterDataDTO ToCharacterDataDTO(this CharacterData source)
        {
            return new CharacterDataDTO()
            {
                BaseCharacterCode = source.BaseCharacterCode,
                Level = source.Level,
                EXP = source.EXP
            };
        }

        static public CompletedAchievementDTO ToCompletedAchievementDTO(this CompletedAchievement source)
        {
            return new CompletedAchievementDTO() { AchievementCode = source.AchievementCode };
        }


        static public GameWorldDataDTO ToGameWorldDataDTO(this GameWorld source)
        {
            return new GameWorldDataDTO()
            {
                Id = source.Id,
                Achievements = source.Achievements.ConvertAll(ToCompletedAchievementDTO)
            };
        }

        static public ItemDTOBase ToItemDTOBase(this Item source)
        {
            switch (source)
            {
                case EquipItem equip:
                    return ToEquipItemDTO(equip);
                default:
                    return new ItemDTO()
                    {
                        Id = source.Id,
                        BaseItemCode = source.BaseItemCode,
                        Count = source.Count,
                        Name = source.Name
                    };
            }
        }

        static private EquipItemDTO ToEquipItemDTO(this EquipItem equip)
        {
            return new EquipItemDTO()
            {
                Id = equip.Id,
                BaseItemCode = equip.BaseItemCode,
                Count = equip.Count,
                Name = equip.Name,
                EXP = equip.EXP
            };
        }
    }
}
