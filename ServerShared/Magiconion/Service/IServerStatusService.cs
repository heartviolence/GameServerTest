using MagicOnion;
using ServerShared.Match;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared.Magiconion.Service
{
    public interface IServerStatusService : IService<IServerStatusService>
    {
        UnaryResult<List<WorldRoomInformationDTO>> GetRoomList(Filter filter);
    }
}
