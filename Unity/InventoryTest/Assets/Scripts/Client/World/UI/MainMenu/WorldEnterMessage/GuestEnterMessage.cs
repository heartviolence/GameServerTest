using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class GuestEnterMessage : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    RectTransform rectTransform;

    [SerializeField]
    EnableAnimation enableAnimation;

    [SerializeField]
    DisableAnimation disableAnimation;

    public async UniTask EnableAnimation()
    {
        await enableAnimation.StartAsync();
    }

    public async UniTask DisableAnimation()
    {
        await disableAnimation.StartAsync();
    }
}
