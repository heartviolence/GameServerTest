
using System.Collections.Generic;
using System;

public class ObjectPool<T> where T : class
{
    public int Size { get => pool.Count; }
    Queue<T> pool = new Queue<T>();

    Func<T> elementFactory;
    public Action<T> AfterDequeue { get; set; }
    public Action<T> BeforeRelease { get; set; }

    public ObjectPool(int size, Func<T> elementFactory)
    {
        this.elementFactory = elementFactory;

        for (int i = 0; i < size; i++)
        {
            Enqueue();
        }
    }

    public void Enqueue()
    {
        pool.Enqueue(elementFactory.Invoke());
    }

    public void Release(T item)
    {
        this.BeforeRelease?.Invoke(item);
        pool.Enqueue(item);
    }

    public void Release(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Release(item);
        }
    }

    public T Dequeue()
    {
        if (pool.Count == 0)
        {
            Enqueue();
        }
        var element = pool.Dequeue();
        this.AfterDequeue?.Invoke(element);
        return element;
    }
}