using MagicOnion;
using MagicOnion.Server;
using ServerShared.Match;
using Server.GameSystems;
using Server.GameSystems.Player.Accounts;
using Server.K8s;
using ServerShared.Magiconion.Service;
using Shared.Server.GameServer.DTO;
using System.Runtime.CompilerServices;

namespace Server.MagicOnion.Services
{
    public class ServerStatusService : ServiceBase<IServerStatusService>, IServerStatusService
    {
        readonly RoomInformations rooms;
        readonly AccountRepository accountRepository;
        readonly ServerStatus serverAddress;
        public ServerStatusService(
            RoomInformations rooms,
            AccountRepository accountRepository,
            ServerStatus serverAddress)
        {
            this.rooms = rooms;
            this.accountRepository = accountRepository;
            this.serverAddress = serverAddress;
        }

        public async UnaryResult<List<WorldRoomInformationDTO>> GetRoomList(Filter filter)
        {
            List<WorldRoomInformationDTO> results = new();
            var roomList = rooms.Dictionary.ToList().Select(pair => pair.Value);

            foreach (var room in roomList)
            {
                var accountData = await accountRepository.GetAccount(room.HostId);
                if (accountData is null)
                {
                    continue;
                }

                results.Add(new WorldRoomInformationDTO()
                {
                    HostId = room.HostId,
                    PlayerIcon = accountData.Icon,
                    PlayerLevel = accountData.AccountLevel,
                    PlayerName = accountData.PlayerName,
                    GuestCount = room.Guests.Count(),
                    ServerAddress = serverAddress.Address
                });
            }

            return results;
        }


    }
}
