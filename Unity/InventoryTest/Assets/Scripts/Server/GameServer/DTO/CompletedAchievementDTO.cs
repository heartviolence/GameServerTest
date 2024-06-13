using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class CompletedAchievementDTO
    {
        [Key(0)]
        public string? AchievementCode { get; set; }
    }
}
