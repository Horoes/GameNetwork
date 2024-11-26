using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySelection : MonoBehaviourPunCallbacks
{
    public AbilityManager abilityManager;

    public void SelectNewAbility(string newAbility)
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // 기존 능력 + 새 능력 추가
        string existingAbility = abilityManager.playerAbilities[playerIndex];
        //abilityManager.playerAbilities[playerIndex]
    }
}
