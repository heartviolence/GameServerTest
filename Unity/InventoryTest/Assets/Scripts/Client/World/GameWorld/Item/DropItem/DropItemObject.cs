using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DropItemObject : MonoBehaviour
{
    public Guid Id { get; set; }
    public bool IsCharacterInRange { get; set; } = false;
    public float TriggerRadius { get; set; } = 2.0f;
}
