using MagicOnion.Server.Hubs;
using MessagePipe;
using Server.K8s;
using ServerShared.Magiconion;

namespace Server.MagicOnion.Hubs
{
    public class MultiplayEnterRequestHub : StreamingHubBase<IWorldEnterRequestHub, IWorldEnterRequestHubReceiver>, IWorldEnterRequestHub
    {
        public struct EnterRequestEvent
        {
            public Guid HostAccountId;
            public Guid GuestId;
        }
        public struct EnterRequestAcceptedEvent
        {
            public Guid HostAccountId;
            public Guid AcceptedPlayerId;
        }

        readonly IAsyncPublisher<EnterRequestEvent> enterRequestPublisher;
        readonly IAsyncSubscriber<EnterRequestAcceptedEvent> enterRequestSubscriber;
        readonly ServerStatus serverAddress;
        readonly ILogger<MultiplayEnterRequestHub> logger;
        IDisposable subscription;
        IGroup room;
        public MultiplayEnterRequestHub(
            IAsyncPublisher<EnterRequestEvent> enterRequestPublisher,
            IAsyncSubscriber<EnterRequestAcceptedEvent> enterRequestSubscriber,
            ServerStatus serverAddress,
            ILogger<MultiplayEnterRequestHub> logger)
        {
            this.enterRequestPublisher = enterRequestPublisher;
            this.enterRequestSubscriber = enterRequestSubscriber;
            this.serverAddress = serverAddress;
            this.logger = logger;
        }

        public async Task EnterRequest(Guid accountId, Guid requesterId)
        {
            logger.LogInformation($"EnterRequest Called\n accountId:{accountId}\n , requesterId: {requesterId}");
            room = await Group.AddAsync(Guid.NewGuid().ToString());

            var bag = DisposableBag.CreateBuilder();
            this.enterRequestSubscriber.Subscribe(async (e, ct) =>
            {
                if (e.AcceptedPlayerId == requesterId)
                {
                    Broadcast(room).OnEnterRequestAccepted(serverAddress.Address, e.HostAccountId);
                }
            }).AddTo(bag);
            subscription = bag.Build();

            await this.enterRequestPublisher.PublishAsync(new EnterRequestEvent
            {
                HostAccountId = accountId,
                GuestId = requesterId
            });
        }

        protected override ValueTask OnDisconnected()
        {
            subscription?.Dispose();
            return base.OnDisconnected();
        }
    }
}
