using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class CharacterDataDTO
    {
        [Key(0)]
        public string? BaseCharacterCode { get; set; }
        [Key(1)]
        public int Level { get; set; }
        [Key(2)]
        public int EXP { get; set; }
    }
}
