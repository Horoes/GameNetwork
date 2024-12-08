using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints; // 스폰 포인트 배열

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
        else
        {
            Debug.LogError("Photon에 연결되어 있지 않습니다!");
        }
    }

    void SpawnPlayer()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            // 마스터 클라이언트는 첫 번째 스폰 포인트
            PhotonNetwork.Instantiate("Player1", spawnPoints[0].position, Quaternion.identity);
        }
        else
        {
            // 다른 클라이언트는 두 번째 스폰 포인트
            PhotonNetwork.Instantiate("Player2", spawnPoints[1].position, Quaternion.identity);
        }

        Debug.Log("플레이어가 스폰되었습니다.");
    }
}
