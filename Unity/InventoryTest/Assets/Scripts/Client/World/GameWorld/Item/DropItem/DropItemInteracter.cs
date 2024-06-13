
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UnityEngine;

public class DropItemInteracter : IInteractable
{
    public readonly DropItemData data;
    public string InteractCode => data.BaseItemCode;

    public Func<UniTask> InteractTask { get; set; }

    public DropItemInteracter(
        DropItemData data,
        Func<UniTask> InteractTask)
    {
        this.data = data;
        this.InteractTask = InteractTask;
    }

    public async UniTask InteractAsync()
    {
        if (InteractTask != null)
        {
            await InteractTask.Invoke();
        }
    }
}