
using Cysharp.Threading.Tasks;
using MessagePipe;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterControllersPresenter : IDisposable
{
    public Dictionary<Guid, CharacterController> Controllers = new();
    readonly IAsyncPublisher<InteractiveListItemAddedEvent> interactItemAdded;
    readonly IAsyncPublisher<InteractiveListItemRemovedEvent> interactItemRemoved;
    readonly IAsyncSubscriber<PlayerEnteredEvent> playerEntered;
    readonly IAsyncSubscriber<PlayerLeavedEvent> PlayerLeaved;
    readonly LoginManager loginManager;
    readonly MainCamera camera;
    readonly IAsyncPublisher<MyCharacterDirectionChangedEvent> characterDirectionChanged;
    readonly IAsyncSubscriber<OtherCharacterDirectionChangedEvent> otherCharacterChanged;
    readonly ControlMode controlMode;
    IDisposable subscription;

    public Guid MyId { get => loginManager.AccountId; }
    public CharacterController MyController
    {
        get
        {
            if (Controllers.TryGetValue(MyId, out var controller))
            {
                return controller;
            }
            return default;
        }
    }

    public CharacterControllersPresenter(
        IAsyncPublisher<InteractiveListItemAddedEvent> interactItemAdded,
        IAsyncPublisher<InteractiveListItemRemovedEvent> interactItemRemoved,
        IAsyncSubscriber<PlayerEnteredEvent> playerEntered,
        IAsyncSubscriber<PlayerLeavedEvent> playerLeaved,
        IAsyncPublisher<MyCharacterDirectionChangedEvent> characterDirectionChanged,
        IAsyncSubscriber<OtherCharacterDirectionChangedEvent> otherCharacterChanged,
        LoginManager loginManager,
        ControlMode controlMode,
        MainCamera camera)
    {
        this.interactItemAdded = interactItemAdded;
        this.interactItemRemoved = interactItemRemoved;
        this.playerEntered = playerEntered;
        this.PlayerLeaved = playerLeaved;
        this.loginManager = loginManager;
        this.camera = camera;
        this.controlMode = controlMode;
        this.characterDirectionChanged = characterDirectionChanged;
        this.otherCharacterChanged = otherCharacterChanged;

        var bag = DisposableBag.CreateBuilder();
        this.playerEntered.Subscribe(async (e, ct) =>
        {
            await AddControllerAsync(e.playerInfo.accountId, e.playerInfo.characterData);
        }).AddTo(bag);

        this.PlayerLeaved.Subscribe(async (e, ct) =>
        {
            RemoveController(e.accountId);
        }).AddTo(bag);
        subscription = bag.Build();
    }

    public async UniTask AddControllerAsync(Guid playerId, CharacterDataDTO characterData)
    {
        if (!Controllers.ContainsKey(playerId))
        {
            var controller = new CharacterController(
                playerId,
                isMyController: loginManager.AccountId == playerId,
                new ControllerState(interactItemAdded, interactItemRemoved),
                controlMode,
                characterDirectionChanged,
                otherCharacterChanged,
                camera);
            Controllers[playerId] = controller;
            var spawnPosition = GameObject.Find("SpawnPosition");
            if (spawnPosition != null)
            {
                await controller.LoadCharacter(spawnPosition.transform.position);
            }
            if (MyId == playerId)
            {
                camera.Attach(controller.Character.cameraHolder);
            }
            await controller.StartAsync(default);
        }
    }

    public void RemoveController(Guid playerId)
    {
        if (Controllers.Remove(playerId, out var controller))
        {
            controller.Dispose();
        }
    }

    public void Dispose()
    {
        subscription?.Dispose();
        var controllers = Controllers.ToList();
        Controllers.Clear();

        foreach (var controller in controllers)
        {
            controller.Value.Dispose();
        }
    }
}