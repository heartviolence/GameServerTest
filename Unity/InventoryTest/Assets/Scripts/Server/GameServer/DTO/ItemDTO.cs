using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.DTO
{
    [Union(0, typeof(ItemDTO))]
    [Union(1, typeof(EquipItemDTO))]
    [MessagePackObject]
    public abstract class ItemDTOBase
    {
        [Key(0)]
        public Guid Id { get; set; }
        [Key(1)]
        public string BaseItemCode { get; set; }
        [Key(2)]
        public int Count { get; set; }
        [Key(3)]
        public string Name { get; set; }
    }

    [MessagePackObject]
    public class ItemDTO : ItemDTOBase
    {

    }

    [MessagePackObject]
    public class EquipItemDTO : ItemDTOBase
    {
        [Key(100)]
        public int EXP { get; set; }
    }



}
