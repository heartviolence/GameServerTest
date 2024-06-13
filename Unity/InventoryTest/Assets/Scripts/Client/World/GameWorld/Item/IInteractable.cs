
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public interface IInteractable
{
    string InteractCode { get; }
    UniTask InteractAsync();
}
