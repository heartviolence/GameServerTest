using Server.EFcore.Models;

namespace Server.GameSystems.Items
{
    public static class ItemExtension
    {
        public static bool IsStackable(this Item item)
        {
            return true;
        }
    }
}
