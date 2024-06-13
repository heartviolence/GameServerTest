using Server.EFcore.Models;
using Server.GameSystems.Items;
using Shared.Server.GameServer.DTO;
using System.Runtime.InteropServices;

namespace ServerShared.Magiconion.Converters
{
    public static class ConvertExtension
    {
        static public DropItemDTO ToDropItemDTO(this DropItem source)
        {
            return new DropItemDTO()
            {
                Id = source.Id,
                OwnerId = source.OwnerId,
                BaseItemCode = source.BaseItemCode,
                Count = source.Count,
                SourceAchievementCode = source.SourceAchievementCode
            };
        }
    }
}
