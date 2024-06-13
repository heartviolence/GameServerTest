using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerShared.Magiconion
{
    public interface IWorldEnterRequestHubReceiver
    {
        void OnEnterRequestAccepted(string serverAddress, Guid hostId);
    }
}
