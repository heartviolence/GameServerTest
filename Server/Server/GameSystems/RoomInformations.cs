using ConcurrentCollections;
using System.Collections.Concurrent;

namespace Server.GameSystems
{
    public class RoomInformation
    {
        public RoomInformation(Guid hostId)
        {
            this.HostId = hostId;
        }
        public Guid HostId { get; }
        public ConcurrentHashSet<Guid> Guests { get; } = new();
    }

    public class RoomInformations
    {
        //hostId(worldId) -> information
        public ConcurrentDictionary<Guid, RoomInformation> Dictionary { get; set; } = new();

        public Action<int> RoomCountChanged { get; set; }
        public RoomInformations()
        {

        }
        public bool TryAddRoom(Guid worldId, RoomInformation room)
        {
            if (Dictionary.TryAdd(worldId, room))
            {
                RoomCountChanged?.Invoke(Dictionary.Count);
                return true;
            }
            return false;
        }

        public bool TryRemoveRoom(Guid worldId, out RoomInformation value)
        {
            if (Dictionary.TryRemove(worldId, out value))
            {
                RoomCountChanged?.Invoke(Dictionary.Count);
                return true;
            }
            return false;
        }
    }
}
