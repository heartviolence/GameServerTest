using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;

public class InteractiveObjectUI : ClickableUI
{
    [SerializeField]
    Image itemImage;

    [SerializeField]
    TMP_Text itemText;

    [SerializeField]
    Image backGround;
    Color defaultColor;

    public Sprite ItemSprite
    {
        set
        {
            this.itemImage.sprite = value;
        }
    }

    public string ItemName
    {
        get { return this.itemText.text; }
        set { this.itemText.text = value; }
    }


    bool focused = false;
    public bool Focused
    {
        get => focused;
        set
        {
            this.focused = value;
            FocusChanged(this.focused);
        }
    }
    Sequence focusAnimation;

    void Awake()
    {
        this.defaultColor = this.backGround.color;
        this.focusAnimation = DOTween.Sequence();
        this.focusAnimation
            .Append(backGround.DOColor(new Color32(200, 200, 200, 200), 1).From(new Color32(140, 140, 140, 200)))
            .Append(backGround.DOColor(new Color32(140, 140, 140, 200), 1))
            .SetLoops(-1, LoopType.Restart).Rewind();
        this.backGround.color = this.defaultColor;
    }

    void FocusChanged(bool value)
    {
        if (focused)
        {
            focusAnimation?.Play();
        }
        else
        {
            focusAnimation?.Rewind();
            this.backGround.color = this.defaultColor;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        Focused = false;
        ItemSprite = null;
        ItemName = string.Empty;
    }
}
