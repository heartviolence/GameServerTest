using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Cysharp.Threading.Tasks;
using Shared.Data;
using Shared.Server.Sheets;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

public class SheetContainer : SheetContainerBase, IAsyncStartable
{
    public static string SheetPath => "./Assets/GameSheet";

    public SheetContainer() : base(UnityLogger.Default) { }

    public ItemSheet Items { get; private set; }
    public AchievementSheet Achievements { get; private set; }
    public InteractiveObjectSheet InteractiveObjects { get; private set; }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        using (var client = new WebClient())
        {
            await client.DownloadFileTaskAsync(new System.Uri($"{ServerAddresses.LoginAndMatchServer}/GameSheet/{nameof(Items)}.json"), $"./Assets/GameSheet/{nameof(Items)}.json");
            await client.DownloadFileTaskAsync(new System.Uri($"{ServerAddresses.LoginAndMatchServer}/GameSheet/{nameof(Achievements)}.json"), $"./Assets/GameSheet/{nameof(Achievements)}.json");
            await client.DownloadFileTaskAsync(new System.Uri($"{ServerAddresses.LoginAndMatchServer}/GameSheet/{nameof(InteractiveObjects)}.json"), $"./Assets/GameSheet/{nameof(InteractiveObjects)}.json");
        }
        await Bake(new JsonSheetConverter(SheetPath));
    }
}

