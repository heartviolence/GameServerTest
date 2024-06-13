
using MessagePipe;
using Shared.Data;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var options = builder.RegisterMessagePipe(/* configure option */);
        builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

        builder.Register<GameInitializeSettings>(Lifetime.Singleton);
        builder.Register<LoginManager>(Lifetime.Singleton);
        builder.Register<GameWorld.Dependencies>(Lifetime.Singleton);
        builder.Register<SheetContainer>(Lifetime.Singleton);
        builder.Register<GameWorldManager>(Lifetime.Singleton);
        builder.Register<MatchServerService>(Lifetime.Singleton);
        builder.Register<WorldAchievementManager>(Lifetime.Singleton);
        builder.Register<WorldDropItemManager>(Lifetime.Singleton);
        builder.Register<InventoryManager>(Lifetime.Singleton);

        builder.RegisterMessageBroker<AchievementAchievedEvent>(options);
        builder.RegisterMessageBroker<MyCharacterDirectionChangedEvent>(options);
        builder.RegisterMessageBroker<OtherCharacterDirectionChangedEvent>(options);
        builder.RegisterMessageBroker<EnterRequestAcceptedEvent>(options);
        builder.RegisterMessageBroker<EnterRequestReceivedEvent>(options);
        builder.RegisterMessageBroker<EnterRequestSendEvent>(options);
        builder.RegisterMessageBroker<InteractiveListItemAddedEvent>(options);
        builder.RegisterMessageBroker<InteractiveListItemRemovedEvent>(options);
        builder.RegisterMessageBroker<GetRewardsEvent>(options);
        builder.RegisterMessageBroker<LogoutEvent>(options);
        builder.RegisterMessageBroker<navigatableUIStateChangedEvent>(options);
        builder.RegisterMessageBroker<PlayerEnteredEvent>(options);
        builder.RegisterMessageBroker<PlayerLeavedEvent>(options);
        builder.RegisterMessageBroker<DropItemAddedEvent>(options);
        builder.RegisterMessageBroker<DropItemRemovedEvent>(options);
        builder.RegisterMessageBroker<InventoryFilterChangedEvent>(options);
        builder.RegisterMessageBroker<InventoryGridSelectedItemChanged>(options);
        builder.RegisterMessageBroker<LoginSuccessedEvent>(options);
        builder.RegisterMessageBroker<LoginFailedEvent>(options);
    }
}