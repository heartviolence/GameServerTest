

using Cysharp.Threading.Tasks;
using Shared.Server.Sheets;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpriteLoadUtil
{
    static public async UniTask<Sprite> LoadItemSpriteAsync(string baseItemCode)
    {
        return await Addressables.LoadAssetAsync<Sprite>(new ItemSpriteAddressablePath(baseItemCode).FullPath);
    }

    static public async UniTask<Sprite> LoadItemRankBackground(ItemSheet.ItemRank itemRank)
    {
        return await Addressables.LoadAssetAsync<Sprite>(new ItemRankBackgroundAddressablePath(itemRank.ToString()).FullPath);
    }

    static public async UniTask<Sprite> LoadGridItemRankBackground(ItemSheet.ItemRank itemRank)
    {
        return await Addressables.LoadAssetAsync<Sprite>(new GridItemRankBackgroundAddressablePath(itemRank.ToString()).FullPath);
    }

    static public async UniTask<Sprite> LoadInteractiveObject(string idCode)
    {
        return await Addressables.LoadAssetAsync<Sprite>(new InteractiveObjectAddressablePath(idCode).FullPath);
    }

    static public void Release<T>(T obj)
    {
        if (obj != null)
        {
            Addressables.Release(obj);
        }
    }

}

