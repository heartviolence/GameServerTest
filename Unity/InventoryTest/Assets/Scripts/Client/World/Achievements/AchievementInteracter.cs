
using Cysharp.Threading.Tasks;
using System;

public class AchievementInteracter : IInteractable
{
    public readonly AchievementObjectBase AchievementObject;
    readonly string interactiveCode;
    public string InteractCode => interactiveCode;
    Func<UniTask> InteractFunc { get; set; }

    public AchievementInteracter(
        AchievementObjectBase achievement,
        string interactiveCode,
        Func<UniTask> interactFunc)
    {
        this.AchievementObject = achievement;
        this.interactiveCode = interactiveCode;
        this.InteractFunc = interactFunc;
    }


    public async UniTask InteractAsync()
    {
        if (InteractFunc != null)
        {
            await InteractFunc.Invoke();
        }
    }
}