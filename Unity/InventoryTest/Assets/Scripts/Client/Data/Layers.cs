
using UnityEngine;

public static class Layers
{
    public static int Default => LayerMask.NameToLayer("Default");
    public static int TransparentFX = LayerMask.NameToLayer("TransparentFX");
    public static int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
    public static int Water = LayerMask.NameToLayer("Water");
    public static int UI = LayerMask.NameToLayer("UI");
    public static int Character = LayerMask.NameToLayer("Character");
}