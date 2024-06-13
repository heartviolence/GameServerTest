

using MessagePack;
using Shared.Server.GameServer.DTO;
using System;

public class Item
{
    public Guid Id { get; set; }
    public string BaseItemCode { get; set; }
    public int Count { get; set; }
    public string Name { get; set; }

    static public Item From(ItemDTOBase dto)
    {
        switch (dto)
        {
            case EquipItemDTO equipItem:
                return EquipItem.From(equipItem);
            default:
                return new Item()
                {
                    Id = dto.Id,
                    BaseItemCode = dto.BaseItemCode,
                    Count = dto.Count,
                    Name = dto.Name
                };
        }
    }
}

public class EquipItem : Item
{
    public int EXP { get; set; }
    static public EquipItem From(EquipItemDTO dto)
    {
        return new EquipItem()
        {
            Id = dto.Id,
            BaseItemCode = dto.BaseItemCode,
            Count = dto.Count,
            Name = dto.Name,
            EXP = dto.EXP
        };
    }
}