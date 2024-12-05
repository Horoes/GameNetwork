using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultManager : MonoBehaviourPunCallbacks
{
    public void EndGame(int losingPlayerActorNumber)
    {
        if(PhotonNetwork.LocalPlayer.ActorNumber==losingPlayerActorNumber)
        {
            // 패배한 플레이어에게 능력 선택 ui표시
            EnableAbilitySelectionUI();
        }
    }

    private void EnableAbilitySelectionUI()
    {
        Debug.Log("패배했습니다. 새 능력을 선택하세요.");
        // 능력 선택 ui 활성화
    }
}
