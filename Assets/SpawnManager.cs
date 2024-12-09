using Photon.Pun;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        GameObject player;
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            player = PhotonNetwork.Instantiate("Player1", spawnPoints[0].position, Quaternion.identity);
        }
        else
        {
            player = PhotonNetwork.Instantiate("Player2", spawnPoints[1].position, Quaternion.identity);
        }

        // 총 인스턴스 생성 및 연결
        GameObject gun = PhotonNetwork.Instantiate("Gun", player.transform.position, Quaternion.identity);
        gun.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
        player.GetComponent<PlayerController>().gun = gun; // 총을 플레이어의 컨트롤러에 할당

        Debug.Log("플레이어와 총이 스폰되었습니다.");
    }
}
