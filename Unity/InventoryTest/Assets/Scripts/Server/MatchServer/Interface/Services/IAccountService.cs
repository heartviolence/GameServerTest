using MagicOnion;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Server.GameServer.Interface.Services
{
    public interface IAccountRegisterService : IService<IAccountRegisterService>
    {
        UnaryResult<bool> RegisterAccount(string loginId, string loginPassword);
    }
}
