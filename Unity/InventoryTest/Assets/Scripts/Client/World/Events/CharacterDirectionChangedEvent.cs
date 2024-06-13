
using System;

public struct MyCharacterDirectionChangedEvent
{
    public UnityEngine.Vector3 currentPosition;
    public UnityEngine.Vector3 currentDirection;
}

public struct OtherCharacterDirectionChangedEvent
{
    public Guid accountId;
    public UnityEngine.Vector3 currentPosition;
    public UnityEngine.Vector3 currentDirection;
}