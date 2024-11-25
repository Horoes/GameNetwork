using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientJoinRoom : MonoBehaviourPunCallbacks
{
    public string roomName;
    private Text roomNameText;
    // Start is called before the first frame update
    void Start()
    {
        roomNameText = transform.Find("RoomName").GetComponent<Text>();

        if(roomName!=null)
        {
            roomName = roomNameText.text;
        }

        GetComponent<Button>().onClick.AddListener(OnRoomButtonClicked);
    }

    private void OnRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(roomName))
        {
            // PhotonInit 싱글톤을 통해 JoinRoom 호출
            PhotonInit.instance.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("roomName이 비어있습니다.");
        }
    }
}
