
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
    // Ŭ����� ���� ����Ʈ

    //���� ó����
    public HashSet<string> Processed { get; } = new();
    //���� ��ó��
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

    //�� ���� ���Խÿ��� ȣ��
    public void Reset()
    {
        Processed.Clear();
        Achieved.Clear();
    }
}