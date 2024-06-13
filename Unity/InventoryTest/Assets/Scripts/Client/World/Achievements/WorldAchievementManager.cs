
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;

public class WorldAchievementManager
{
    readonly IAsyncPublisher<AchievementAchievedEvent> achievementAchieved;

    // key->achievementCode
    // 클리어된 업적 리스트

    //보상 처리됨
    public HashSet<string> Processed { get; } = new();
    //보상 미처리
    public HashSet<string> Achieved { get; } = new();

    public WorldAchievementManager(
        IAsyncPublisher<AchievementAchievedEvent> achievementAchieved)
    {
        this.achievementAchieved = achievementAchieved;
    }

    public async UniTask Achieve(string achievementCode)
    {
        if (string.IsNullOrEmpty(achievementCode))
        {
            return;
        }

        Achieved.Add(achievementCode);
        await achievementAchieved.PublishAsync(new AchievementAchievedEvent { AchievementCode = achievementCode });
    }

    public bool Process(string achievementCode)
    {
        if (Achieved.Remove(achievementCode))
        {
            return Processed.Add(achievementCode);
        }
        return false;
    }

    //새 월드 진입시에만 호출
    public void Reset()
    {
        Processed.Clear();
        Achieved.Clear();
    }
}