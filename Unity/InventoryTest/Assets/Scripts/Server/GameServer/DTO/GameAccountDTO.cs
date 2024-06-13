using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class GameAccountDTO
    {
        [Key(0)]
        public Guid AccountId { get; set; }
        [Key(1)]
        public int UID { get; set; }
        [Key(2)]
        public List<ItemDTOBase> Inventory { get; set; } = new();
        [Key(3)]
        public GameWorldDataDTO GameWorld { get; set; }
        [Key(4)]
        public List<CharacterDataDTO> CharacterDatas { get; set; } = new List<CharacterDataDTO>();
    }
}
