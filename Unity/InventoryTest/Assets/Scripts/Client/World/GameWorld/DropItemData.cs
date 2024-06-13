using Shared.Server.GameServer.DTO;
using System;

public class DropItemData
{
    public Guid Id { get; set; } = Guid.Empty;
    public string BaseItemCode { get; set; }
    public int Count { get; set; }
    public string SourceAchievementCode { get; set; } = string.Empty;

    public bool IsSpawned { get; set; } = false;

    public static DropItemData From(DropItemDTO dto)
    {
        return new DropItemData()
        {
            Id = dto.Id,
            BaseItemCode = dto.BaseItemCode,
            Count = dto.Count,
            SourceAchievementCode = dto.SourceAchievementCode
        };
    }
}