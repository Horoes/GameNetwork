using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine;
using Photon.Pun.Demo.Cockpit;
using System.Linq;

public class MakingRoom : MonoBehaviourPunCallbacks
{
    private PhotonInit photoninit;
    public Text roomCount;
    public GameObject[] roomPrefabsArray; // Room 프리팹들
    public Button createRoomBtn; // 방 생성 버튼
    public InputField roomNameInput; // 방 이름을 입력받을 InputField
    public int currentRoomIndex = 0;    // 현재 생성한 방 갯수 인덱스
    public Image Limitroom;             // 방 생성 최대 도달 이미지
    public GameObject RoomPopup;
    private int maxPlayers = 2;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    private void Start()
    {

        createRoomBtn.onClick.AddListener(OnCreateButtonClick);

    }

    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    cachedRoomList.Clear(); // 기존 목록 초기화

    //    foreach (var roomInfo in roomList)
    //    {
    //        if (!roomInfo.RemovedFromList) // 삭제되지 않은 방만 추가
    //        {
    //            cachedRoomList.Add(roomInfo);
    //        }
    //    }
    //}

    void OnCreateButtonClick()                      // '생성' 버튼을 눌렀을 때 방 생성하는 함수
    {

        string roomName = roomNameInput.text;       // 방 이름 기입

        if (string.IsNullOrEmpty(roomName))         // 만약 방 이름을 아무것도 기입하지 않았다면
        {
            Debug.Log("방 제목을 입력하세요.");
            return;
        }

        // 중복 이름 검증
        if (cachedRoomList.Any(room => room.Name == roomName))
        {
            Debug.Log("이미 존재하는 방 이름입니다.");
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
                }

            }
            RoomOptions roomOptions = new RoomOptions   // 생성되는 방 option 설정
            {
                MaxPlayers = 2, // 방 최대 정원 수
                EmptyRoomTtl = 0    // 방이 비게 되면 즉시 삭제
            };
            PhotonNetwork.CreateRoom(roomName, roomOptions);    // 방 생성
        }
        else
        {   
            Debug.Log("모든 방이 생성되었습니다.");
            StartCoroutine(FullRoom());
        }
    }

    IEnumerator FullRoom()          // LimitRoom Image가 On된 후 2초뒤 Off
    {
        Limitroom.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        Limitroom.gameObject.SetActive(false);
    }

    //public override void OnJoinedRoom()             // 방 입장 시 콜백
    //{
    //    Debug.Log("방에 입장했습니다.");
    //    UpdatePlayerCount(currentRoomIndex); // 방에 입장한 후 인원 수 업데이트 (currentRoomIndex가 증가된 이후이므로 -1로 보정)
    //    // 방에 입장한 후 currentRoomIndex 증가시키기
    //    currentRoomIndex++;

    //}

   
}
