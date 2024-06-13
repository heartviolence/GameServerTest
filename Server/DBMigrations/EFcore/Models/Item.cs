using System.ComponentModel.DataAnnotations.Schema;

namespace Server.EFcore.Models
{
    public class Item
    {
        public Guid Id { get; set; }
        public required string BaseItemCode { get; set; }
        public int Count { get; set; }
        public required string Name { get; set; }
        public GameAccount Owner { get; set; }
    }

    public class EquipItem : Item
    {
        public required int EXP { get; set; }
    }

}
