
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;

public struct PlayerEnteredEvent
{
    public (Guid accountId, CharacterDataDTO characterData) playerInfo;
}

public struct PlayerLeavedEvent
{
    public Guid accountId;
}