
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Events;
using VContainer.Unity;

public class RoomInfoUIListPresenter
{
    readonly RoomInfoUIList ui;
    readonly MatchServerService server;
    readonly LoginManager loginManager;

    public RoomInfoUIListPresenter(
        RoomInfoUIList ui,
        MatchServerService server,
        LoginManager loginManager)
    {
        this.ui = ui;
        this.server = server;
        this.loginManager = loginManager;
    }

    const int maxGuestCount = 4;
    public async UniTask Refresh(CancellationToken cancellation)
    {
        ui.Clear();
        var roomList = await server.GetMultiplayWorldList(default);
        if (roomList == null)
        {
            return;
        }
        roomList.RemoveAll(e => e.HostId == loginManager.AccountId);

        foreach (var room in roomList)
        {
            var child = ui.NewChild();
            child.playerName.text = room.PlayerName;
            child.guestCount.text = $"{room.GuestCount + 1} / {maxGuestCount}";
            child.playerLevel.text = $"LV.{room.PlayerLevel}";
            child.enterButton.onClick.AddListener(() =>
            {
                server.EnterRequest(room, loginManager.AccountId).Forget();
            });
            child.gameObject.SetActive(true);
        }
    }
}