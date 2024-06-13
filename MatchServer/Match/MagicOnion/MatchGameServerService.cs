using k8s;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using ServerShared.K8sResourceDefinition;
using ServerShared.Match.MagicOnion.Channel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Server.EFcore.Contexts;
using Server.EFcore.Models;
using ServerShared.Magiconion;
using ServerShared.Magiconion.Service;
using Shared.Server.GameServer.DTO;
using Shared.Server.MatchServer.Interface.Hubs;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ServerShared.Match.MagicOnion
{
    public class MatchGameServerService : ServiceBase<IMatchGameServerService>, IMatchGameServerService
    {
        ILogger logger;
        Kubernetes kubernetes;
        const string localhost = "https://localhost:9999";

        public MatchGameServerService(ILogger<MatchGameServerService> logger)
        {
            this.logger = logger;
#if (FINAL || TEST)
            var config = KubernetesClientConfiguration.InClusterConfig();
            kubernetes = new Kubernetes(config);
#endif
        }

        /// <summary>
        /// 접속가능한서버 검색
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<List<(int capacity, string address)>> GetAvailableServerList()
        {
            List<(int capacity, string address)> results = new();
#if (FINAL || TEST)
            var gClient = new GenericClient(kubernetes, group: "agones.dev", version: "v1", plural: "gameservers");
            var gameServers = await gClient.ListNamespacedAsync<CustomResourceList<AgonesGameServer>>("default");

            try
            {
                foreach (var gs in gameServers.Items)
                {
                    if (gs.Status.State != "Ready")
                    {
                        continue;
                    }

                    if (gs.Metadata.Annotations.TryGetValue("agones.dev/sdk-initialized", out var isInitializedAnnotaion))
                    {
                        if (isInitializedAnnotaion != "true")
                        {
                            continue;
                        }
                    }

                    string serverAddress = string.Empty;
                    if (!gs.Metadata.Annotations.TryGetValue("agones.dev/sdk-GSAddress", out serverAddress))
                    {
                        continue;
                    }
                    string serverPort = gs.Status.Ports[0].Port.ToString();                   

                    int roomCapacity = int.Parse(gs.Metadata.Annotations["agones.dev/sdk-roomcapacity"]);
                    if (roomCapacity < 95)
                    {
                        results.Add(new(roomCapacity, serverAddress));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "error from GetAvailableServerList");
                return default;
            }
#else
            results.Add(new(0, localhost));
#endif
            return results;
        }

        /// <summary>
        /// Multiplay Room 검색
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async UnaryResult<List<WorldRoomInformationDTO>> GetMultiplayWorldListAsync(Filter filter)
        {
            var results = new List<WorldRoomInformationDTO>();
#if (FINAL || TEST)
            var gClient = new GenericClient(kubernetes, group: "agones.dev", version: "v1", plural: "gameservers");
            var gameServers = await gClient.ListNamespacedAsync<CustomResourceList<AgonesGameServer>>("default");
            try
            {
                foreach (var gs in gameServers.Items)
                {
                    if (gs.Status.State != "Ready")
                    {
                        continue;
                    }

                    if (gs.Metadata.Annotations.TryGetValue("agones.dev/sdk-initialized", out var isInitializedAnnotaion))
                    {
                        if (isInitializedAnnotaion != "true")
                        {
                            continue;
                        }
                    }
                    string serverAddress = string.Empty;
                    if (!gs.Metadata.Annotations.TryGetValue("agones.dev/sdk-GSAddress", out serverAddress))
                    {
                        continue;
                    }
                    string serverPort = gs.Status.Ports[0].Port.ToString();

                    var channel = GrpcChannels.ForAddress(serverAddress);
                    var service = MagicOnionClient.Create<IServerStatusService>(channel);
                    results.AddRange(await service.GetRoomList(filter));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "error from GetMultiplayerWorldList");
                return default;
            }
#else
            try
            {
                var channel = GrpcChannels.ForAddress(localhost);
                var service = MagicOnionClient.Create<IServerStatusService>(channel);
                results.AddRange(await service.GetRoomList(filter));
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "error from GetMultiplayerWorldList");
                return default;
            }
#endif
            return results;
        }
    }
}
