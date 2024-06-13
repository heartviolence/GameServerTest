
using System.Threading;

public enum MainUIState
{
    Unknown,
    GameWorld,
    Inventory,
    Multiplay
}

public struct navigatableUIStateChangedEvent
{
    public MainUIState state;
}