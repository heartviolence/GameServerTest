
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using VContainer.Unity;

public class GameWorldMainUIPresenter : NavigatableUIPresenterBase, IAsyncStartable
{
    readonly MainMenuBarPresenter menubar;
    readonly InteractiveListPresenter interactiveListPresenter;
    readonly RewardListPresenter awardListPresenter;
    readonly GuestEnterMessagePresenter guestEnterMessagePresenter;
    readonly GameWorldMainUI ui;

    public GameWorldMainUIPresenter(
        MainMenuBarPresenter menubar,
        InteractiveListPresenter interactiveListPresenter,
        RewardListPresenter awardListPresenter,
        GuestEnterMessagePresenter guestEnterMessagePresenter,
        GameWorldMainUI ui,
        IAsyncSubscriber<navigatableUIStateChangedEvent> uiChangeSubscriber) : base(uiChangeSubscriber, MainUIState.GameWorld)
    {
        this.menubar = menubar;
        this.interactiveListPresenter = interactiveListPresenter;
        this.awardListPresenter = awardListPresenter;
        this.guestEnterMessagePresenter = guestEnterMessagePresenter;
        this.ui = ui;
    }

    public override async UniTask DisableAsync()
    {
        menubar.DisableAsync().Forget();
        await ui.disableAnimation.StartAsync();
    }

    public override async UniTask EnableAsync()
    {
        menubar.EnableAsync().Forget();
        await ui.enableAnimation.StartAsync();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await menubar.StartAsync(cancellation);
        await interactiveListPresenter.StartAsync(cancellation);
        await awardListPresenter.StartAsync(cancellation);
        await guestEnterMessagePresenter.StartAsync(cancellation);
    }
}
