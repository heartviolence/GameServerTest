
using MagicOnion;
using ServerShared.Match;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;

namespace Shared.Server.MatchServer.Interface.Hubs
{
    public interface IMatchGameServerService : IService<IMatchGameServerService>
    {
        UnaryResult<List<(int capacity, string address)>> GetAvailableServerList();
        UnaryResult<List<WorldRoomInformationDTO>> GetMultiplayWorldListAsync(Filter filter);
    }
}

