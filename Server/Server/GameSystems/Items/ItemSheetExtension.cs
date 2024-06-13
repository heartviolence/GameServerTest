using Server.EFcore.Models;
using Shared.Server.Sheets;

namespace Server.GameSystems.Items
{
    public static class ItemSheetExtension
    {
        static public Item ToItem(this ItemSheet.Row baseItem)
        {
            switch (baseItem.ItemType)
            {
                case ItemSheet.ItemType.Equip:
                    return new EquipItem()
                    {
                        BaseItemCode = baseItem.Id,
                        Count = 1,
                        Name = baseItem.Name,
                        EXP = 0
                    };
                default:
                    return new Item()
                    {
                        BaseItemCode = baseItem.Id,
                        Count = 1,
                        Name = baseItem.Name
                    };
            }

            return default;
        }
    }
}
