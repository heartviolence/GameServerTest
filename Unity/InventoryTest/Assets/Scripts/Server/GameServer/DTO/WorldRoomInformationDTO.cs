
using MessagePack;
using System;

namespace Shared.Server.GameServer.DTO
{
    [MessagePackObject]
    public class WorldRoomInformationDTO
    {
        [Key(0)]
        public Guid HostId { get; set; }
        [Key(1)]
        public string PlayerIcon { get; set; }
        [Key(2)]
        public string PlayerName { get; set; }
        [Key(3)]
        public int PlayerLevel { get; set; }
        [Key(4)]
        public int GuestCount { get; set; }
        [Key(5)]
        public string ServerAddress { get; set; }

    }
}