using Cysharp.Threading.Tasks;
using DG.Tweening;
using Shared.Server.Sheets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryGridItem : ClickableUI, IPointerEnterHandler, IPointerExitHandler
{
    public Image ItemImage;
    public Image ItemRankBackground;
    public TMP_Text ItemCount;
    [SerializeField]
    Image WhiteBackGround;

    bool focused = false;
    public bool Focused
    {
        get => this.focused;
        set
        {
            if (this.focused != value)
            {
                this.focused = value;
                FocusChanged(value);
            }
        }
    }

    Sequence pointerEnterAnimation;

    private void Awake()
    {
        pointerEnterAnimation = DOTween.Sequence();
        pointerEnterAnimation
            .Append(WhiteBackGround.DOColor(new Color32(255, 255, 255, 255), 1.0f).From(new Color32(255, 255, 255, 0)))
            .Rewind();
    }

    void FocusChanged(bool newValue)
    {
        if (newValue)
        {
            pointerEnterAnimation?.Rewind();
            WhiteBackGround.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            pointerEnterAnimation?.Rewind();
            WhiteBackGround.color = new Color32(255, 255, 255, 0);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!focused)
        {
            pointerEnterAnimation?.Play();
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (!focused)
        {
            pointerEnterAnimation?.Rewind();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        pointerEnterAnimation?.Rewind();
        Focused = false;
        ItemRankBackground.sprite = null;
        ItemImage.sprite = null;
        WhiteBackGround.color = new Color32(255, 255, 255, 0);
    }
}
