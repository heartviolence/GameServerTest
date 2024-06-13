
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : UIListElementBase
{
    [SerializeField]
    Image itemImage;

    [SerializeField]
    TMP_Text itemText;

    public Sprite ItemSprite
    {
        get => itemImage.sprite;
        set => itemImage.sprite = value;
    }

    public string ItemText
    {
        get => itemText.text;
        set => itemText.text = value;
    }

    [SerializeField]
    EnableAnimation enableAnimation;

    [SerializeField]
    DisableAnimation disableAnimation;

    public async UniTask EnableAnimation(float waitSecond)
    {
        await enableAnimation.StartAsync();
        await UniTask.WaitForSeconds(waitSecond, cancellationToken: this.GetCancellationTokenOnDestroy());
        await disableAnimation.StartAsync();
    }

    public override void Dispose()
    {
        ItemSprite = null;
        ItemText = null;
        base.Dispose();
    }
}