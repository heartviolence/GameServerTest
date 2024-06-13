
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public abstract class UIListBase<T> : MonoBehaviour where T : UIListElementBase
{
    [SerializeField]
    protected GameObject childElementPrefab;

    private ObjectPool<T> pool;
    protected ObjectPool<T> Pool
    {
        get
        {
            if (this.pool == null)
            {
                this.pool = CreatePool();
            }
            return this.pool;
        }
    }

    [HideInInspector]
    public List<T> children = new();
    protected virtual Transform parentTransform { get => this.transform; }
    protected virtual int poolSize => 1;

    public T this[int i]
    {
        get { return children[i]; }
    }

    virtual public T NewChild()
    {
        var uiElement = this.Pool.Dequeue();
        this.children.Add(uiElement);
        return uiElement;
    }

    virtual public void RemoveChild(T item)
    {
        if (children.Remove(item))
        {
            Pool.Release(item);
        }
    }

    public void Clear()
    {
        this.Pool.Release(this.children);
        this.children.Clear();
    }

    protected ObjectPool<T> CreatePool(Func<T> factory = null, Action<T> afterDequeue = null, Action<T> beforeRelease = null)
    {
        if (factory == null)
        {
            factory = () =>
            {
                var uiElement = Instantiate(childElementPrefab).GetComponentInChildren<T>();
                uiElement.transform.SetParent(this.parentTransform, false);
                uiElement.gameObject.SetActive(false);

                return uiElement;
            };
        }

        if (afterDequeue == null)
        {
            afterDequeue = (element) =>
            {
                element.transform.SetAsLastSibling();
                if (element is IInitializable initializable)
                {
                    initializable.Initialize();
                }
            };
        }

        if (beforeRelease == null)
        {
            beforeRelease = (element) =>
            {
                element.gameObject.SetActive(false);
                element.Dispose();
            };
        }

        ObjectPool<T> objectPool = new(this.poolSize, factory);
        objectPool.AfterDequeue += afterDequeue;
        objectPool.BeforeRelease += beforeRelease;

        return objectPool;
    }
}