using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class GameWorldDataDTO
    {
        [Key(0)]
        public Guid Id { get; set; }
        [Key(1)]
        public List<CompletedAchievementDTO> Achievements { get; set; } = new List<CompletedAchievementDTO>();
    }
}
