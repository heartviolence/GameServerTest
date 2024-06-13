
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer.Unity;

public class DropItemPresenter : IAsyncStartable, IDisposable
{
    readonly IAsyncSubscriber<DropItemAddedEvent> addEvent;
    readonly IAsyncSubscriber<DropItemRemovedEvent> removeEvent;
    readonly CharacterControllersPresenter controllers;
    readonly WorldDropItemManager worldDropItems;
    IDisposable subscription;

    Dictionary<string, DropItemSpawner> spawners = new();
    Dictionary<Guid, DropItemObject> itemObjects = new();
    CancellationTokenSource lifeCts = new();

    public DropItemPresenter(
        IAsyncSubscriber<DropItemAddedEvent> addEvent,
        IAsyncSubscriber<DropItemRemovedEvent> removeEvent,
        CharacterControllersPresenter controllers,
        WorldDropItemManager worldDropItems)
    {
        this.addEvent = addEvent;
        this.removeEvent = removeEvent;
        this.controllers = controllers;
        this.worldDropItems = worldDropItems;

        var bag = DisposableBag.CreateBuilder();
        addEvent.Subscribe(async (e, ct) =>
        {
            if (spawners.TryGetValue(e.addedItem.SourceAchievementCode, out var spawner))
            {
                await SpawnDropItem(spawner, e.addedItem);
            }
        }).AddTo(bag);

        removeEvent.Subscribe(async (e, ct) =>
        {
            if (itemObjects.Remove(e.id, out var value))
            {
                Addressables.Release(value.gameObject);
            }
            controllers.MyController.state.RemoveInteractiveItem(e.removedItem);
        }).AddTo(bag);
        subscription = bag.Build();
    }

    public void Dispose()
    {
        subscription?.Dispose();
        lifeCts.Cancel();
        lifeCts.Dispose();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        TriggerCheck(lifeCts.Token).Forget();
        RefreshItemSpawners();
    }

    async UniTask TriggerCheck(CancellationToken ct)
    {
        int layerMask = (1 << Layers.Character);

        while (true)
        {
            await UniTask.WaitForFixedUpdate(ct);

            foreach (var pair in itemObjects)
            {
                var itemId = pair.Key;
                var itemObject = pair.Value;
                if (itemObject == null)
                {
                    continue;
                }
                var beforeState = itemObject.IsCharacterInRange;
                var afterState = false;

                Collider[] colliders = Physics.OverlapSphere(itemObject.transform.position, itemObject.TriggerRadius, layerMask);
                foreach (var collider in colliders)
                {
                    if (collider.CompareTag(Tags.PlayerCharacter))
                    {
                        //캐릭터가 트리거 범위안에 존재함
                        afterState = true;
                    }
                }

                itemObject.IsCharacterInRange = afterState;
                if (worldDropItems.InteractableItems.TryGetValue(itemId, out var interacter))
                {
                    if (beforeState != afterState)
                    {
                        if (afterState)
                        {
                            controllers.MyController.state.AddInteractiveItem(interacter);
                        }
                        else
                        {
                            controllers.MyController.state.RemoveInteractiveItem(interacter);
                        }
                    }
                }
                else
                {
                    RemoveDropItemObject(itemId);
                }
            }
        }
    }

    public bool RegisterSpawner(DropItemSpawner spawner)
    {
        return spawners.TryAdd(spawner.AchievementCode, spawner);
    }
    public bool EraseSpawner(string key)
    {
        return spawners.Remove(key);
    }

    public void RemoveDropItemObject(Guid id)
    {
        if (itemObjects.Remove(id, out var value))
        {
            if (worldDropItems.InteractableItems.TryGetValue(id, out var interacter))
            {
                controllers.MyController.state.RemoveInteractiveItem(interacter);
            }
            Addressables.Release(value.gameObject);
        }
    }

    async UniTask SpawnDropItem(DropItemSpawner spawner, DropItemData data)
    {
        var dropItemObject = await Addressables.InstantiateAsync("Assets/Prefabs/Cuboid.prefab", position: spawner.transform.position, rotation: Quaternion.identity);
        var dropItemScript = dropItemObject.GetComponent<DropItemObject>();
        dropItemScript.Id = data.Id;
        dropItemObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 70, -70));
        itemObjects.Add(data.Id, dropItemScript);
    }

    void RefreshItemSpawners()
    {
        spawners.Clear();
        DropItemSpawner[] findObjects = UnityEngine.Object.FindObjectsByType<DropItemSpawner>(FindObjectsSortMode.None);

        foreach (var findObject in findObjects)
        {
            spawners.Add(findObject.AchievementCode, findObject);
        }
    }
}