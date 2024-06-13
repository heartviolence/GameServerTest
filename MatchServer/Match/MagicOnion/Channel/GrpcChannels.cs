using Grpc.Net.Client;
using System.Collections.Concurrent;

namespace ServerShared.Match.MagicOnion.Channel
{
    public static class GrpcChannels
    {
        static public ConcurrentDictionary<string, GrpcChannel> Channels = new(); //server address->channel

        static public GrpcChannel ForAddress(string address)
        {
            return Channels.GetOrAdd(address, GrpcChannel.ForAddress(address));
        }
    }
}
