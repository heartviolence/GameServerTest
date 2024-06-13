using Cysharp.Net.Http;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Chest : AchievementObjectBase
{
    [SerializeField]
    float openDuration = 0.5f;
    [SerializeField]
    float destroyDelay = 2.0f;

    [SerializeField]
    GameObject chestTop;

    [SerializeField]
    GameObject chest;

    [SerializeField]
    DropItemSpawner dropItemSpawner;

    bool isOpened = false;

    public override string AchievementCode => dropItemSpawner.AchievementCode;

    public async UniTask Open()
    {
        if (isOpened)
        {
            return;
        }
        isOpened = true;
        TriggerEnable = false;

        chestTop.GetComponent<BoxCollider>().enabled = false;
        await chestTop.transform.DOLocalRotate(new Vector3(150, 0, 0), openDuration);
        await UniTask.WaitForSeconds(destroyDelay);
        Destroy(this.chest);
    }

    public override async UniTask AchieveAsync()
    {
        await Open();
    }

    public override async UniTask DestroyAsync()
    {
        Destroy(this.gameObject);
    }
}
