
using MagicOnion;
using Shared.Server.GameServer.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Shared.Server.GameServer.Interface.Hubs
{
    public interface ILoginHub : IStreamingHub<ILoginHub, ILoginHubReceiver>
    {
        Task ServerConnect();
        Task<(Guid token, GameAccountDTO? dto)> Login(string userId, string password);
        Task LogOut();
        Task<bool> ChangePlayerName(string name);
        Task<List<ItemDTOBase>?> GetInventory();
    }
}