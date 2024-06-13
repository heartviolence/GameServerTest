
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class AchievementObjectBase : MonoBehaviour
{
    public abstract string AchievementCode { get; }

    [HideInInspector]
    public bool IsCharacterInRange { get; set; }

    public float TriggerRadius { get; set; } = 1.0f;

    public bool TriggerEnable { get; set; } = true;
    public abstract UniTask AchieveAsync();

    public abstract UniTask DestroyAsync();
}