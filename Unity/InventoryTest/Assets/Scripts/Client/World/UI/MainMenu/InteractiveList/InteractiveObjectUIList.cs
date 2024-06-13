using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveObjectUIList : UIListBase<InteractiveObjectUI>
{
    [SerializeField]
    GameObject contentObject;

    [SerializeField]
    RectTransform scrollbar;

    public ScrollRect scrollRect;
    protected override Transform parentTransform => contentObject.transform;
    protected override int poolSize => 5;

    private void Awake()
    {
        this.scrollbar.sizeDelta = new Vector2(7, 0);
    }
}
