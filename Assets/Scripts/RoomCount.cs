using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomCount : MonoBehaviourPunCallbacks
{
    public Text roomCountText; // UI 텍스트 참조
    private MakingRoom makingRoom;  // MakingRoom 클래스 참조

    void Start()
    {
        // 초기 방 갯수를 표시합니다.
        UpdateRoomCount();
    }

    // 방이 생성되었을 때 호출되는 콜백 함수
    public override void OnCreatedRoom()
    {
        UpdateRoomCount();
    }

    //// 방이 제거되었을 때 호출되는 콜백 함수
    //public override void OnLeftRoom()
    //{
    //    UpdateRoomCount();
    //}

    void UpdateRoomCount()
    {
        // 현재 생성된 방 갯수를 가져와서 UI에 표시합니다.
        int roomCount = makingRoom.currentRoomIndex;
        roomCountText.text = "생성된 방 갯수: " + roomCount;
    }


}
