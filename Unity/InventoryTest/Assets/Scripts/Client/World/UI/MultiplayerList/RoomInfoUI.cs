using Cysharp.Threading.Tasks;
using Shared.Server.GameServer.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfoUI : UIListElementBase
{
    public TMP_Text playerName;
    public TMP_Text guestCount;
    public TMP_Text playerLevel;
    public Image playerIcon;
    public Button enterButton;

    public override void Dispose()
    {
        playerName.text = "";
        guestCount.text = "";
        playerLevel.text = "";
        enterButton.onClick.RemoveAllListeners();
        base.Dispose();
    }
}
