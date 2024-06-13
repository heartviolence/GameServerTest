
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using VContainer.Unity;

public class AchievementPresenter : IAsyncStartable, IDisposable
{
    readonly IAsyncSubscriber<AchievementAchievedEvent> achievementAchieved;
    readonly WorldAchievementManager achievementManager;
    readonly GameWorldManager gameWorldManager;
    readonly CharacterControllersPresenter controllers;
    readonly SheetContainer sheet;
    Dictionary<string, AchievementInteracter> achievementInteracters = new();

    CancellationTokenSource lifeCts = new();
    IDisposable subscription;

    public AchievementPresenter(
        WorldAchievementManager achievementManager,
        GameWorldManager gameWorldManager,
        CharacterControllersPresenter controllers,
        SheetContainer sheet,
        IAsyncSubscriber<AchievementAchievedEvent> achievementAchieved)
    {
        this.achievementAchieved = achievementAchieved;
        this.achievementManager = achievementManager;
        this.gameWorldManager = gameWorldManager;
        this.controllers = controllers;
        this.sheet = sheet;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        UpdateAchievementObjects();
        TriggerCheck(lifeCts.Token).Forget();
        var bag = DisposableBag.CreateBuilder();
        achievementAchieved.Subscribe(async (e, ct) =>
        {
            await AchieveObjectAsync(e.AchievementCode);
        }).AddTo(bag);
        subscription = bag.Build();
    }

    public async UniTask AchieveObjectAsync(string achievementCode)
    {
        if (achievementInteracters.TryGetValue(achievementCode, out var interacter))
        {
            await interacter.AchievementObject.AchieveAsync();
            achievementInteracters.Remove(achievementCode);
            controllers.MyController.state.RemoveInteractiveItem(interacter);
            achievementManager.Process(achievementCode);
        }
    }

    void UpdateAchievementObjects()
    {
        var findObjects = UnityEngine.Object.FindObjectsByType<AchievementObjectBase>(FindObjectsSortMode.None);

        foreach (var obj in findObjects)
        {
            //달성된 업적이라면 삭제
            if (achievementManager.Processed.Contains(obj.AchievementCode))
            {
                obj.DestroyAsync();
                continue;
            }

            if (achievementManager.Achieved.Contains(obj.AchievementCode))
            {
                AchieveObjectAsync(obj.AchievementCode).Forget();
            }

            //등록되지않은 업적이면 등록
            if (!achievementInteracters.ContainsKey(obj.AchievementCode))
            {
                try
                {
                    Func<UniTask> interactFunc = async () =>
                    {
                        await gameWorldManager.CurrentWorld.AchieveAsync(obj.AchievementCode);
                    };
                    string interactCode = sheet.Achievements[obj.AchievementCode].InteractCode;
                    achievementInteracters.Add(obj.AchievementCode, new AchievementInteracter(obj, interactCode, interactFunc));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }

    async UniTask TriggerCheck(CancellationToken ct)
    {
        int layerMask = (1 << Layers.Character);
        var destroyedObjects = new List<KeyValuePair<string, AchievementInteracter>>();
        while (true)
        {
            await UniTask.WaitForFixedUpdate(ct);

            foreach (var pair in achievementInteracters)
            {
                var achievementObject = pair.Value.AchievementObject;
                if (achievementObject == null)
                {
                    destroyedObjects.Add(pair);
                    continue;
                }
                var beforeState = achievementObject.IsCharacterInRange;
                var afterState = false;

                if (achievementObject.TriggerEnable)
                {
                    Collider[] colliders = Physics.OverlapSphere(achievementObject.transform.position, achievementObject.TriggerRadius, layerMask);
                    foreach (var collider in colliders)
                    {
                        if (collider.CompareTag(Tags.PlayerCharacter))
                        {
                            //캐릭터가 트리거 범위안에 존재함
                            afterState = true;
                        }
                    }
                }

                achievementObject.IsCharacterInRange = afterState;

                if (beforeState != afterState)
                {
                    if (afterState)
                    {
                        controllers.MyController.state.AddInteractiveItem(pair.Value);
                    }
                    else
                    {
                        controllers.MyController.state.RemoveInteractiveItem(pair.Value);
                    }
                }
            }

            foreach (var pair in destroyedObjects)
            {
                achievementInteracters.Remove(pair.Key);
                controllers.MyController.state.RemoveInteractiveItem(pair.Value);
            }
            destroyedObjects.Clear();
        }
    }

    public void Dispose()
    {
        lifeCts.Cancel();
        lifeCts.Dispose();
        subscription?.Dispose();
    }
}