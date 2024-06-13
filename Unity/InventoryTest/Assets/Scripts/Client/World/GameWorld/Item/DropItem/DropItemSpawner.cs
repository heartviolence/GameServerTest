using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

public class DropItemSpawner : MonoBehaviour
{
    [SerializeField]
    string achievementCode = string.Empty;
    public string AchievementCode { get => achievementCode; }
}
