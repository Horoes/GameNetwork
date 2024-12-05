using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class ClientJoinRoom : MonoBehaviourPunCallbacks
{
    public string roomName;
    public Image FullRoom;
    private Text roomNameText;

    // Start is called before the first frame update
    void Start()
    {
        roomNameText = transform.Find("RoomName").GetComponent<Text>();

        if (roomNameText != null)
        {
            roomName = roomNameText.text;
            Debug.Log($"ClientJoinRoom Start: roomName 초기화됨 -> {roomName}");
        }
        else
        {
            Debug.LogError("ClientJoinRoom Start: RoomName Text 컴포넌트를 찾을 수 없습니다.");
        }

        // 동적으로 버튼 클릭 이벤트 등록
        GetComponent<Button>().onClick.AddListener(() => OnRoomButtonClicked());
    }

    //private void OnRoomButtonClicked()
    //{
    //    if (!string.IsNullOrEmpty(roomName))
    //    {
    //        // 현재 방 정보 가져오기 
    //        RoomInfo clickedRoom = GetRoomInfoByName(roomName);

    //        if (clickedRoom != null)
    //        {
    //            if (clickedRoom.PlayerCount >= clickedRoom.MaxPlayers)
    //            {
    //                StartCoroutine(CheckFullRoom()); // 알림 창
    //                Debug.LogWarning("방의 최대 인원에 도달하여 입장할 수 없습니다.");
    //            }
    //            else
    //            {
    //                Debug.Log("방 입장이 가능합니다.");
    //                // 방 입장
    //                PhotonInit.instance.JoinRoom(roomName);
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogWarning("클릭한 방을 찾을 수 없습니다.");
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("roomName이 비어있습니다.");
    //    }
    //}
    private void OnRoomButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            Debug.LogWarning("현재 로비에 입장하지 않은 상태입니다.");
            return;
        }
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("roomName이 비어 있습니다!");
            return;
        }

        Debug.Log($"클릭한 방 이름: {roomName}");

        RoomInfo clickedRoom = GetRoomInfoByName(roomName);

        if (clickedRoom != null)
        {
            if (clickedRoom.PlayerCount >= clickedRoom.MaxPlayers)
            {
                StartCoroutine(CheckFullRoom());
                Debug.LogWarning("방의 최대 인원에 도달하여 입장할 수 없습니다.");
            }
            else
            {
                Debug.Log($"방 입장이 가능합니다. 방 이름: {roomName}");
                PhotonInit.instance.JoinRoom(roomName); // JoinRoom 호출
            }
        }
        else
        {
            Debug.LogWarning($"클릭한 방({roomName})을 찾을 수 없습니다.");
        }
    }
    private RoomInfo GetRoomInfoByName(string targetRoomName)
    {
        if (PhotonInit.instance == null)
        {
            Debug.LogError("PhotonInit.instance가 null입니다!");
            return null;
        }

        if (PhotonInit.instance.myList == null)
        {
            Debug.LogError("PhotonInit.instance.myList가 null입니다!");
            return null;
        }

        foreach (RoomInfo roomInfo in PhotonInit.instance.myList)
        {
            if (roomInfo.Name == targetRoomName)
            {
                return roomInfo;
            }
        }
        return null;
    }


    IEnumerator CheckFullRoom() // 방 인원이 다 차게 되면 Image ui로 2초간 알림
    {
        FullRoom.gameObject.SetActive(true);

        yield return new WaitForSeconds(2.0f);
        FullRoom.gameObject.SetActive(false);
    }
}
