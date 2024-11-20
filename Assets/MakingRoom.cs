using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using Photon.Pun.Demo.Cockpit;

public class MakingRoom : MonoBehaviourPunCallbacks
{
    public Text roomCount;
    public GameObject[] roomPrefabsArray; // Room 프리팹들
    public Button createRoomBtn; // 방 생성 버튼
    public InputField roomNameInput; // 방 이름을 입력받을 InputField
    public int currentRoomIndex = 0;    // 현재 생성한 방 갯수 인덱스 
    private int[] roomCounts;           // 방 프리팹 갯수
    private int maxPlayers=2;
    private void Start()
    {
        roomCounts = new int[roomPrefabsArray.Length];
        createRoomBtn.onClick.AddListener(OnCreateButtonClick);
     
    }
    
    void OnCreateButtonClick()                      // '생성' 버튼을 눌렀을 때 방 생성하는 함수
    {
        string roomName = roomNameInput.text;       // 방 이름 기입

        if (string.IsNullOrEmpty(roomName))         // 만약 방 이름을 아무것도 기입하지 않았다면
        {
            Debug.Log("방 제목을 입력하세요.");
            return;
        }

        if (currentRoomIndex < roomPrefabsArray.Length)
        {
            roomPrefabsArray[currentRoomIndex].SetActive(true);         // 차례로 room 프리팹 on

            // 자식 오브젝트에서 "RoomName"이라는 이름을 가진 오브젝트를 찾아 그 Text 컴포넌트를 가져오기
            Transform roomNameTransform = roomPrefabsArray[currentRoomIndex].transform.Find("RoomName");
            if (roomNameTransform != null)
            {
                Text roomNameText = roomNameTransform.GetComponent<Text>();
                if (roomNameText != null)
                {
                    roomNameText.text = roomName;               // 기입한 방 이름을 새로 개설된 대기 방 이름으로 설정

                    RoomOptions roomOptions = new RoomOptions();                // 최대 플레이어수를 정하기 위한 room option
                    roomOptions.MaxPlayers = 2; // 방의 최대 플레이어 수 설정

                    PhotonNetwork.CreateRoom(roomName, roomOptions);                    // 방 생성
                }
            }
            roomCount.text = currentRoomIndex+1 + "/7";
            currentRoomIndex++;  // 방 생성이 성공적으로 이루어진 이후에 인덱스를 증가시킵니다.
        }
        else
        {
            Debug.Log("모든 방이 생성되었습니다.");
        }
    }
  
    public override void OnJoinedRoom()             // 방 입장 시 콜백
    {
        Debug.Log("방에 입장했습니다.");
        UpdatePlayerCount(currentRoomIndex); // 방에 입장한 후 인원 수 업데이트 (currentRoomIndex가 증가된 이후이므로 -1로 보정)
       
    }

    void UpdatePlayerCount(int roomIndex)
    {
        Transform headCountTransform = roomPrefabsArray[roomIndex].transform.Find("HeadCount");
        int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        if (headCountTransform != null)
        {
            Text headCountText = headCountTransform.GetComponent<Text>();
            if (headCountText != null)
            {
                headCountText.text = $"{currentPlayers}/{maxPlayers}";
            }
        }
        
    }



    //public override void OnJoinedRoom()
    //{
    //    Debug.Log("방에 참가했습니다: " + PhotonNetwork.CurrentRoom.Name);
    //    OnPlayerJoin(currentRoomIndex - 1);
    //}

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("새로운 플레이어가 방에 입장했습니다: " + newPlayer.NickName);
       
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("플레이어가 방을 떠났습니다: " + otherPlayer.NickName);
        //OnPlayerLeave(currentRoomIndex - 1);
    }


    //public void OnPlayerLeave(int roomIndex)
    //{
    //    if (roomIndex < playerCounts.Length && playerCounts[roomIndex] > 0)
    //    {
    //        playerCounts[roomIndex]--;
    //        UpdatePlayerCount(roomIndex);
    //    }
    //}

   

    void StartGame(int roomIndex)
    {
        Debug.Log($"방 {roomIndex + 1} 게임이 시작됩니다.");
        // 게임 씬을 로드합니다
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

   
}
