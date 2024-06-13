using MagicOnion;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shared.Server.GameServer.Interface.Hubs
{
    public interface IGameWorldHub : IStreamingHub<IGameWorldHub, IGameWorldHubRecevier>
    {
        Task<GameWorldDataDTO> EnterAsync(Guid worldId);
        ValueTask AcceptGuestAsync(Guid guestId);
        Task<bool> Achieve(string achievementCode);
        Task LootDropItem(Guid itemId);
        Task CharacterDirectionChanged(Vector3 currentPosition, Vector3 currentDir);
        Task<List<(Guid accountId, CharacterDataDTO characterData)>> GetRoomMembers();
        Task<bool> LoginCheck(Guid token, Guid accountId);
    }
}
