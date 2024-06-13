
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using ServerShared.Match;
using MessagePipe;
using Microsoft.Extensions.Logging;
using ServerShared.Magiconion;
using Shared.Data;
using Shared.Server.GameServer.DTO;
using Shared.Server.MatchServer.Interface.Hubs;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchServerService : IWorldEnterRequestHubReceiver
{
    ChannelBase matchServerChannel = null;
    public Action EnterRequestAccepted { get; set; }


    IMatchGameServerService service = null;
    IMatchGameServerService Service
    {
        get
        {
            if (matchServerChannel == null)
            {
                matchServerChannel = GrpcChannelx.ForAddress(ServerAddresses.LoginAndMatchServer);
            }

            if (service == null)
            {
                service = MagicOnionClient.Create<IMatchGameServerService>(matchServerChannel);
            }
            return service;
        }
    }
    readonly IAsyncPublisher<EnterRequestAcceptedEvent> enterRequestAccepeted;


    public MatchServerService(
        IAsyncPublisher<EnterRequestAcceptedEvent> enterRequestAccepeted)
    {
        this.enterRequestAccepeted = enterRequestAccepeted;
    }

    public async UniTask<List<(int capacity, string address)>> GetAvailableServerList()
    {
        return await Service.GetAvailableServerList();
    }

    public async UniTask EnterRequest(WorldRoomInformationDTO roomInformation, Guid accountId)
    {
        if (roomInformation == null ||
            string.IsNullOrEmpty(roomInformation.ServerAddress) ||
            accountId == Guid.Empty)
        {
            return;
        }

        try
        {
            var channel = GrpcChannelx.ForAddress(roomInformation.ServerAddress);
            var hub = await StreamingHubClient.ConnectAsync<IWorldEnterRequestHub, IWorldEnterRequestHubReceiver>(channel, this);

            await hub.EnterRequest(roomInformation.HostId, accountId);
            delayDisposeHubAsync(10.0f, hub).Forget();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return;
        }
    }

    public async UniTask<List<WorldRoomInformationDTO>> GetMultiplayWorldList(Filter filter)
    {
        return await Service.GetMultiplayWorldListAsync(filter);
    }

    private async UniTask delayDisposeHubAsync(float delay, IWorldEnterRequestHub hub)
    {
        await UniTask.WaitForSeconds(delay);
        await hub.DisposeAsync();
    }


    public void OnEnterRequestAccepted(string serverAddress, Guid hostId)
    {
        EnterRequestAccepted?.Invoke();
        enterRequestAccepeted.Publish(new EnterRequestAcceptedEvent()
        {
            serverAddress = serverAddress,
            hostId = hostId
        });
    }
}