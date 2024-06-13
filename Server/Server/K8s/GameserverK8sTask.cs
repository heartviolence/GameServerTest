
using Agones;
using Amazon.Route53.Model;
using Amazon.Route53;
using k8s;
using Server.GameSystems;
using static Agones.Dev.Sdk.GameServer.Types.Status.Types;
using Agones.Dev.Sdk;

namespace Server.K8s
{
    public class GameserverK8sTask : IHostedService
    {
        private readonly ILogger<GameserverK8sTask> logger;
        readonly ServerStatus server;
        readonly RoomInformations rooms;
        AgonesSDK agones;
        private Timer? healthpingTimer = null;
        object healthpingTimerLock = new();
        const int healthpingInterval = 4;
        int maxWorldCount = 10;

        public GameserverK8sTask(
            ILogger<GameserverK8sTask> logger,
            ServerStatus server,
            RoomInformations rooms)
        {
            this.logger = logger;
            this.server = server;
            this.rooms = rooms;
            agones = new(logger: logger);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await K8sInitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task K8sInitializeAsync()
        {
            await ReadyAsync();
            await agones.SetAnnotationAsync("roomcapacity", "0");
            rooms.RoomCountChanged += RoomCountChanged;

            var gs = await agones.GetGameServerAsync();
            while (string.IsNullOrEmpty(gs.Status.Address))
            {
                await Task.Delay(1000);
                gs = await agones.GetGameServerAsync();
            }
            string domainName = $"{gs.ObjectMeta.Name}.fireflyagonestest.com";
            var port = gs.Status.Ports[0].Port_;
            server.Address = "https://" + domainName + ":" + port;
            await agones.SetAnnotationAsync("GSAddress", server.Address);

            logger.LogInformation("Gameserver Address : " + gs.Status.Address);
            IAmazonRoute53 route53Client = new AmazonRoute53Client();

            ResourceRecordSet recordSet = new ResourceRecordSet
            {
                Name = domainName,
                TTL = 300,
                Type = RRType.CNAME,
                ResourceRecords = new List<ResourceRecord> { new ResourceRecord { Value = gs.Status.Address } }
            };

            Change change1 = new Change
            {
                ResourceRecordSet = recordSet,
                Action = ChangeAction.CREATE
            };

            ChangeBatch changeBatch = new ChangeBatch
            {
                Changes = new List<Change> { change1 }
            };

            ChangeResourceRecordSetsRequest recordsetRequest = new ChangeResourceRecordSetsRequest
            {
                HostedZoneId = "Z07787221ZPCNQK7O5MW0",
                ChangeBatch = changeBatch
            };

            ChangeResourceRecordSetsResponse recordsetResponse = await route53Client.ChangeResourceRecordSetsAsync(recordsetRequest);

            GetChangeRequest changeRequest = new GetChangeRequest
            {
                Id = recordsetResponse.ChangeInfo.Id
            };

            while ((await route53Client.GetChangeAsync(changeRequest)).ChangeInfo.Status == ChangeStatus.PENDING)
            {
                logger.LogInformation("Change is pending.");
                await Task.Delay(5000);
            }
            logger.LogInformation("route53 record created");
            await agones.SetAnnotationAsync("initialized", "true");
        }

        public async Task<bool> ReadyAsync()
        {
            await agones.ReadyAsync();

            lock (healthpingTimerLock)
            {
                healthpingTimer?.Dispose();
            }

            for (int i = 0; i < 5; i++)
            {
                var status = await agones.ReadyAsync();
                if (status.StatusCode == Grpc.Core.StatusCode.OK)
                {
                    logger.LogInformation("Gameserver Ready OK");
                    lock (healthpingTimerLock)
                    {
                        healthpingTimer = new Timer(HealthPing, null, TimeSpan.Zero, TimeSpan.FromSeconds(healthpingInterval));
                    }
                    return true;
                }
                else
                {
                    logger.LogInformation($"Gameserver Ready Failed, tryCount:{i + 1}");
                }
            }

            return false;
        }

        private async void HealthPing(object? state)
        {
            var status = await agones.HealthAsync();
            if (status.StatusCode == Grpc.Core.StatusCode.OK)
            {
                //logger.LogInformation("health OK");
            }
            else
            {
                logger.LogInformation("health Fail");
                logger.LogInformation("statusCode : " + status.StatusCode.ToString());
                logger.LogInformation(status.Detail);
            }
        }

        private void RoomCountChanged(int current)
        {
            var capacity = Math.Round((double)(current * 100) / maxWorldCount, 1);
            agones.SetAnnotationAsync("roomcapacity", capacity.ToString());
        }
    }
}
