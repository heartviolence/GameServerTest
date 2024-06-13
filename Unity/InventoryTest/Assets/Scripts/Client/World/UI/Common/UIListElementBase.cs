
using System;
using UnityEngine;

public abstract class UIListElementBase : MonoBehaviour, IDisposable
{
    public Action OnDispose { get; set; }

    public virtual void Dispose()
    {
        OnDispose?.Invoke();
        OnDispose = null;
    }

    public void SetActive(bool state)
    {
        this.gameObject.SetActive(state);
    }
}