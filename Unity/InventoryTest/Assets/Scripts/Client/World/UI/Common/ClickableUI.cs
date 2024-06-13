using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class ClickableUI : UIListElementBase, IPointerClickHandler
{
    public Action<PointerEventData> Clicked { get; set; }

    public override void Dispose()
    {
        Clicked = null;
        base.Dispose();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(eventData);
    }
}