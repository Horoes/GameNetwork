using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPun
{
    // 현 로컬 플레이어 능력
    private string localPlayerAbility;

    // 로컬 플레이어 능력 선택
    public void SelectAbility(string ability)
    {
        localPlayerAbility = ability;


       
    }
   private void SetPlayerAbility(Player player, string ability)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["Ability"] = ability;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        //UpdateAbilityUI(newAbility);

    }
}
