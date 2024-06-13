using MessagePack;
using System;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class DropItemDTO
    {
        [Key(0)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Key(1)]
        public Guid OwnerId { get; set; } = Guid.Empty;
        [Key(2)]
        public string BaseItemCode { get; set; }
        [Key(3)]
        public int Count { get; set; }
        [Key(4)]
        public string SourceAchievementCode { get; set; } = string.Empty;
    }
}
