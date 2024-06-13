using Cathei.BakingSheet;

namespace Server.GameSystems.Sheets
{
    public class GameSheets
    {
        public GameSheetContainer Sheets { get; set; }

        readonly string SheetPath = "./GameSheets";
        public GameSheets(ILogger<GameSheets> logger)
        {
            Sheets = new GameSheetContainer(logger);
            var converter = new JsonSheetConverter(SheetPath);

            Sheets.Bake(converter).Wait();
        }
    }
}
