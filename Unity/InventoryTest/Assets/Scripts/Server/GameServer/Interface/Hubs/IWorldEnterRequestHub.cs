using MagicOnion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared.Magiconion
{
    public interface IWorldEnterRequestHub : IStreamingHub<IWorldEnterRequestHub, IWorldEnterRequestHubReceiver>
    {
        Task EnterRequest(Guid accountId, Guid requesterId);
    }
}
