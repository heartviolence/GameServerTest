
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryCategoryUI : ClickableUI, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Image background;

    [SerializeField]
    Image icon;

    bool focused = false;
    public bool Focused
    {
        get => this.focused;
        set
        {
            if (this.focused == value)
            {
                return;
            }

            this.focused = value;
            if (focused)
            {
                FocusAnimation();
            }
            else
            {
                DefaultAnimation();
            }
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!Focused)
        {
            background?.DOColor(new Color32(70, 70, 70, 70), 0.5f);
            icon?.DOColor(new Color32(255, 255, 255, 210), 0.5f);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        DefaultAnimation();
    }

    private void DefaultAnimation()
    {
        if (!Focused)
        {
            background?.DOColor(new Color32(0, 0, 0, 40), 0.5f);
            icon?.DOColor(new Color32(255, 255, 255, 170), 0.5f);
        }
    }

    private void FocusAnimation()
    {
        background?.DOColor(new Color32(210, 210, 210, 255), 0.5f);
        icon?.DOColor(new Color32(0, 0, 0, 255), 0.5f);
    }
}