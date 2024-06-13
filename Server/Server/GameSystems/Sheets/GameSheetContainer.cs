using Cathei.BakingSheet;
using Shared.Server.Sheets;

namespace Server.GameSystems.Sheets
{
    public class GameSheetContainer : SheetContainerBase
    {
        public GameSheetContainer(ILogger logger) : base(logger) { }
        public ItemSheet Items { get; set; }
        public AchievementSheet Achievements { get; set; }
    }
}
