using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Shared.Server.Sheets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ExcelPostprocessor : AssetPostprocessor
{
    static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // automatically run postprocessor if any excel file is imported
        string excelAsset = importedAssets.FirstOrDefault(x => x.EndsWith(".xlsx"));

        if (excelAsset != null)
        {
            var excelPath = Path.GetDirectoryName(excelAsset);
            var sheetContainer = new SheetContainer();

            // create excel converter from path
            var excelConverter = new ExcelSheetConverter(excelPath, TimeZoneInfo.Utc);

            // bake sheets from excel converter
            await sheetContainer.Bake(excelConverter);

            // (optional) verify that data is correct
            sheetContainer.Verify(
#if BAKINGSHEET_ADDRESSABLES
                    new AddressablePathVerifier(),
#endif
                new ResourcePathVerifier()
            );

            // create json converter to path
            var jsonConverter = new JsonSheetConverter("../../MatchServer/wwwroot/GameSheet");
            var jsonConverter2 = new JsonSheetConverter("../../Server/Server/GameSheets");

            // save datasheet to streaming assets
            await sheetContainer.Store(jsonConverter);
            await sheetContainer.Store(jsonConverter2);

            AssetDatabase.Refresh();

            Debug.Log("Excel sheet converted.");
        }
    }
}
