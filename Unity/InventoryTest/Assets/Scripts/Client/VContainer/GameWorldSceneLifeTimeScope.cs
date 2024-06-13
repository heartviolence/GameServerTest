
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameWorldSceneLifeTimeScope : LifetimeScope
{
    [SerializeField]
    GameWorldUIs uis;

    [SerializeField]
    MainCamera mainCamera;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<GameworldScenePresenter>();
        builder.Register<GameworldUIPresenter>(Lifetime.Singleton);
        builder.Register<UINavigator>(Lifetime.Singleton);
        builder.RegisterComponent(mainCamera);

        //CharacterControl
        builder.Register<CharacterControllersPresenter>(Lifetime.Singleton);
        builder.Register<ControlMode>(Lifetime.Singleton);

        //Achievement
        builder.Register<AchievementPresenter>(Lifetime.Singleton);

        //dropItem
        builder.Register<DropItemPresenter>(Lifetime.Singleton);        

        //Inventory        
        builder.Register<InventoryUIPresenter>(Lifetime.Singleton);
        builder.Register<InventoryGridPresenter>(Lifetime.Singleton);
        builder.Register<InventoryTopPresenter>(Lifetime.Singleton);
        builder.Register<InventoryItemDescriptionPresenter>(Lifetime.Singleton);
        builder.RegisterComponent(uis.inventory);
        builder.RegisterComponent(uis.inventory.grid);
        builder.RegisterComponent(uis.inventory.topUI);
        builder.RegisterComponent(uis.inventory.description);

        //multiplay
        builder.Register<MultiplayerListUIPresenter>(Lifetime.Singleton);
        builder.Register<RoomInfoUIListPresenter>(Lifetime.Singleton);
        builder.Register<GuestEnterMessagePresenter>(Lifetime.Singleton);
        builder.RegisterComponent(uis.multiplayerList);
        builder.RegisterComponent(uis.multiplayerList.roomInfoList);

        //gamworldMainUI
        builder.Register<GameWorldMainUIPresenter>(Lifetime.Singleton);
        builder.Register<MainMenuBarPresenter>(Lifetime.Singleton);
        builder.Register<RewardListPresenter>(Lifetime.Singleton);
        builder.Register<InteractiveListPresenter>(Lifetime.Singleton);
        builder.RegisterComponent(uis.gameworldMainUI);
        builder.RegisterComponent(uis.gameworldMainUI.rewardList);
        builder.RegisterComponent(uis.gameworldMainUI.mainMenuBar);
        builder.RegisterComponent(uis.gameworldMainUI.interactiveObjectList);
        builder.RegisterComponent(uis.gameworldMainUI.guestEnterMessage);
    }
}