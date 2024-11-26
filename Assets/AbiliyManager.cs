using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviourPunCallbacks
{
    public string[] playerAbilities = new string[2];    // 각 플레이어 능력

    // 능력 선택
    public void SelectInitialAbility(string abilityName)
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        playerAbilities[playerIndex] = abilityName;

        photonView.RPC("SyncAbility", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, abilityName);
        Debug.Log($"플레이어 {playerIndex + 1}이 능력을 선택하였습니다: {abilityName}");
    }

    [PunRPC]
    public void SyncAbility(int actorNumber,string abilityName)
    {
        int playerIndex = actorNumber - 1;
        playerAbilities[playerIndex] = abilityName;
        Debug.Log($"플레이어 {playerIndex + 1} 능력 동기화..: {abilityName}");
    }
}
