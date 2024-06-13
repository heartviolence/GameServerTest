using Shared.Server.GameServer.DTO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared.Server.GameServer.Interface.Hubs
{
    public interface IGameWorldHubRecevier
    {
        void OnGuestEnterRequest(Guid guestId);

        void OnOtherPlayerEnter(Guid playerId, CharacterDataDTO characterData);
        void OnOtherPlayerLeave(Guid playerId);

        void OnDropItemAdded(List<DropItemDTO> dropItemDtos);
        void OnDropItemRemoved(DropItemDTO dropItemDtos);
        void OnOtherCharacterDirectionChanged(Guid playerId, Vector3 currentPosition, Vector3 currentVector);
        void OnAchievementCompleted(string achievementCode);
        void OnRewardsReceived(Guid accountId, List<ItemDTOBase> items);
    }
}
