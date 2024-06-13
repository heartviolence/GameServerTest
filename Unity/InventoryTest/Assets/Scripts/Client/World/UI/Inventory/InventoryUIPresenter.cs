
using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using VContainer.Unity;

public class InventoryUIPresenter : NavigatableUIPresenterBase, IAsyncStartable
{
    readonly InventoryTopPresenter top;
    readonly InventoryGridPresenter grid;
    readonly InventoryUI inventoryUI;
    readonly LoginManager loginManager;
    readonly InventoryItemDescriptionPresenter itemDescriptionPresenter;

    public InventoryUIPresenter(
        InventoryTopPresenter top,
        InventoryGridPresenter grid,
        InventoryItemDescriptionPresenter itemDescriptionPresenter,
        InventoryUI inventoryUI,
        LoginManager loginManager,
        IAsyncSubscriber<navigatableUIStateChangedEvent> navChanged) : base(navChanged, MainUIState.Inventory)
    {
        this.top = top;
        this.grid = grid;
        this.inventoryUI = inventoryUI;
        this.loginManager = loginManager;
        this.itemDescriptionPresenter = itemDescriptionPresenter;
    }

    public override async UniTask DisableAsync()
    {
        await inventoryUI.disableAnimation.StartAsync();
    }

    public override async UniTask EnableAsync()
    {
        await loginManager.InventoryUpdate();
        await grid.RefreshUIAsync();
        await inventoryUI.enableAnimation.StartAsync();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await itemDescriptionPresenter.StartAsync(cancellation);
        await grid.StartAsync(cancellation);
        await top.StartAsync(cancellation);
    }
}