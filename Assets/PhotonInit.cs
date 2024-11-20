using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    // �̱��� ������ ������ �ؾ� ��
    public static PhotonInit instance;

    public InputField playerInput;
    bool isGameStart = false;
    bool isLoggIn = false;
    string playerName = "";
    string connectionState = "";
    public string chatMessage ;
    Text chatText;
    ScrollRect scroll_rect = null;
    PhotonView pv;

    Text connectionInfoText;

    [Header("LobbyCanvas")] public GameObject LobbyCanvas;
    public GameObject LobbyPanel;
    public GameObject MakeRoomPanel;
    public GameObject RoomPanel;
    public InputField RoomInput;
    public InputField RoomPwInput;
    public Toggle PwToggle;
    public GameObject PwPanel;
    public GameObject PwErrorLog;
    public GameObject PwConfirmBtn;
    public GameObject PwPanelCloseBtn;
    public InputField PwCheckIF;
    public bool LockState = false;
    public string privateroom;
    public Button[] CellBtn;
    public Button PreviousBtn;
    public Button NextBtn;
    public Button CreateRoomBtn;
    public GameObject[] roomPrefabsArray;
    public int hashtablecount;
    private int currentRoomIndex = 0;
    private int[] roomCounts;
    // �������� �� ����Ʈ�� �����ϴ� ����
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple, roomnumber;

    private void Awake()
    {
  
        PhotonNetwork.GameVersion = "MyFps 1.0";
        PhotonNetwork.ConnectUsingSettings();

        // ���� 2���� ������ �ε��� �Ǳ⶧���� UI ó���� �غ���
        if(GameObject.Find("ChatText") != null)
            chatText = GameObject.Find("ChatText").GetComponent<Text>();

        if (GameObject.Find("Scroll View") != null)
            scroll_rect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

        if (GameObject.Find("ConnectionInfoText") != null)
            connectionInfoText = GameObject.Find("ConnectionInfoText").GetComponent<Text>();
        
        connectionState = "마스터 서버에 접속 중...";

        if(connectionInfoText)
            connectionInfoText.text = connectionState;
        //�Ʒ��� �Լ��� ����Ͽ� ���� ��ȯ �Ǵ��� ���� �Ǿ��� �ν��Ͻ��� �ı����� �ʴ´�.

        //DontDestroyOnLoad(gameObject);
    }
    void OnCreateButtonClick()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Photon 서버에 연결되어 있지 않습니다. 잠시 후 다시 시도해 주세요.");
            return;
        }

        string roomName = RoomInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.Log("방 제목을 입력하세요.");
            return;
        }

        
    }
  
    public static PhotonInit Instance
    {
        get
        {
            if(!instance)
            {
                instance = FindObjectOfType(typeof(PhotonInit)) as PhotonInit;

                if (instance == null)
                    Debug.Log("no singleton obj");
            }

            return instance;
        }
    }



    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby");
        //PhotonNetwork.CreateRoom("MyRoom");
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinRoom("MyRoom");
    }

    public void Connect()
    {
        Debug.Log("성공");
        if (PhotonNetwork.IsConnected)
        {
            connectionState = "연결 중...";
            if (connectionInfoText)
                connectionInfoText.text = connectionState;

            LobbyPanel.SetActive(false);
            RoomPanel.SetActive(true);
            
            PhotonNetwork.JoinLobby();
            //���� ������ ���� �Ǹ� �κ� ����! ���� �����ϰ� ���°Ŵ� ���� ����!
            //��� ���� ��Ȳ�� �����ִ� �г��� �����ش�
            //PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectionState = "연결 실패 : 서버에 접속하지 못했습니다\n재접속 시도 중...";
            if (connectionInfoText)
                connectionInfoText.text = connectionState;
            PhotonNetwork.ConnectUsingSettings();
        }

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionState = "No Room";
        if (connectionInfoText)
            connectionInfoText.text = connectionState;
        Debug.Log("No Room");
        //����� �κе� �ּ� ó��
        //PhotonNetwork.CreateRoom("MyRoom");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("OnCreateRoomFailed:"+returnCode + "-"+message);
    }

    //public override void OnJoinedLobby()
    //{
    //    base.OnJoinedLobby();
    //    connectionState = "Joined Lobby";
    //    if (connectionInfoText)
    //        connectionInfoText.text = connectionState;
    //    myList.Clear();

    //    Debug.Log("OnJoinedLobby:" );

    //}

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        connectionState = "Joined Room";
        if (connectionInfoText)
            connectionInfoText.text = connectionState;
        Debug.Log("Joined Room");
        isLoggIn = true;
        PlayerPrefs.SetInt("LogIn", 1);
        PlayerPrefs.SetString("Player", playerName);

        //SceneManager.LoadScene("SampleScene");
        //PhotonNetwork.LoadLevel("SampleScene");

    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("LogIn", 0);
    }

    private void Update()
    {
        if (PlayerPrefs.GetInt("LogIn") == 1)
        {
            isLoggIn = true;
           
        }
        if (isGameStart == false && SceneManager.GetActiveScene().name == "SampleScene" && isLoggIn == true)
        {
            isGameStart = true;
            if (GameObject.Find("ChatText") != null)
                chatText = GameObject.Find("ChatText").GetComponent<Text>();

            if (GameObject.Find("Scroll View") != null)
                scroll_rect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

            playerName = PlayerPrefs.GetString("Player");
            PlayerPrefs.SetString("Player", "");
            StartCoroutine(CreatePlayer());
        }
    }

    IEnumerator CreatePlayer()
    {
        while(!isGameStart)
        {
            yield return new WaitForSeconds(0.5f);
        }

        GameObject tempPlayer = PhotonNetwork.Instantiate("PlayerDagger",
                                    new Vector3(0, 0, 0),
                                    Quaternion.identity,
                                    0);
        print(playerName);
        tempPlayer.GetComponent<PlayerCtrl>().SetPlayerName(playerName);
        pv = GetComponent<PhotonView>();

        yield return null;
    }

    private void OnGUI()
    {
        GUILayout.Label(connectionState);
    }

    public void SetPlayerName()
    {
        Debug.Log(playerInput.text + "님이 입력하였습니다!");

        if(isGameStart == false && isLoggIn == false)
        {
            playerName = playerInput.text;
            
            playerInput.text = string.Empty;
            Debug.Log("connect 시도!" + isGameStart + ", " + isLoggIn);
            Connect();
            
        }
        else
        {
            chatMessage = playerInput.text;
            playerInput.text = string.Empty;
            //ShowChat(chatMessage);
            pv.RPC("ChatInfo", RpcTarget.All, chatMessage);
        }
        
    }

    public void ShowChat(string chat)
    {
        chatText.text += chat + "\n";
        scroll_rect.verticalNormalizedPosition = 0.0f;
    }

    [PunRPC]
    public void ChatInfo(string sChat)
    {
        ShowChat(sChat);
    }

    #region �� ���� �� ���� ���� �޼���
    public void CreateRoomBtnOnClick()
    {
        MakeRoomPanel.SetActive(true);
    }

    public void OKBtnOnClick()
    {
        MakeRoomPanel.SetActive(false);
    }


    //public void CreateRoom()
    //{
    //     PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Game" + Random.Range(0, 100) : RoomInput.text,
    //            new RoomOptions { MaxPlayers = 20 });
    //    LobbyPanel.SetActive(false);
    //}

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        connectionState = "������ ������ ���� ��...";
        if (connectionInfoText)
            connectionInfoText.text = connectionState;
        isGameStart = false;
        isLoggIn = false;
        PlayerPrefs.SetInt("LogIn", 0);
       
    }


    //���ο� �� �����
    //public void CreateNewRoom()           // 교수님 방 생성 코드 (CreateNewRoom은 커스텀 메서드라, 비밀번호 같이 추가 기능을 걸 때)
    //{
        
    //    RoomOptions roomOptions = new RoomOptions();
    //    roomOptions.MaxPlayers = 20;
    //    roomOptions.CustomRoomProperties = new Hashtable()
    //    {
    //        {"password", RoomPwInput.text}
    //    };
    //    roomOptions.CustomRoomPropertiesForLobby = new string[] { "password" };

    //    if (PwToggle.isOn)
    //    {
    //        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Game" + Random.Range(0, 100) : "*" + RoomInput.text,
    //            roomOptions);
    //    }

    //    else
    //    {
    //        PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Game" + Random.Range(0, 100) : RoomInput.text,
    //            new RoomOptions { MaxPlayers = 20 });
    //    }

    //    MakeRoomPanel.SetActive(false);
    //    //LobbyCanvas.SetActive(false);
        
        
        
    //}


    // ����ư -2 , ����ư -1 , �� ����
    public void MyListClick(int num)
    {
       
        if (num == -2)
        {
            --currentPage;
            MyListRenewal();
        }
        else if (num == -1)
        {
            ++currentPage;
            MyListRenewal();
        }

        else if (myList[multiple + num].CustomProperties["password"] != null)
        {
            PwPanel.SetActive(true);
        }
        else
        {
            PhotonNetwork.JoinRoom(myList[multiple + num].Name);
            MyListRenewal();
            
        }
       
    }

    public void RoomPw(int number)
    {
        switch (number)
        {
            case 0:
                roomnumber = 0;
                break;
            case 1:
                roomnumber = 1;
                break;
            case 2:
                roomnumber = 2;
                break;
            case 3:
                roomnumber = 3;
                break;

            default:
                break;
        }
    
    
    }

    public void EnterRoomWithPW()
    {
       if ((string)myList[multiple + roomnumber].CustomProperties["password"] == PwCheckIF.text)
        {
            PhotonNetwork.JoinRoom(myList[multiple + roomnumber].Name);
            MyListRenewal();
            PwPanel.SetActive(false);
        }

        else
        {
            StartCoroutine("ShowPwWrongMsg");
        }
        
        
    }

    IEnumerator ShowPwWrongMsg()
    {
        if (!PwErrorLog.activeSelf)
        {
            PwErrorLog.SetActive(true);
            yield return new WaitForSeconds(3.0f);
            PwErrorLog.SetActive(false);
        }
    }

    void MyListRenewal()
    {
        // 페이지 설정
        maxPage = (myList.Count % CellBtn.Length == 0)
            ? myList.Count / CellBtn.Length
            : myList.Count / CellBtn.Length + 1;
        // 이전 및 다음 버튼 활성화 설정
        if (PreviousBtn != null)
        {
            PreviousBtn.interactable = (currentPage <= 1) ? false : true;
        }

        if (NextBtn != null)
        {
            NextBtn.interactable = (currentPage >= maxPage) ? false : true;
        }
        // 현재 페이지에 표시할 방 목록 설정
        multiple = (currentPage - 1) * CellBtn.Length;
        for (int i = 0; i < CellBtn.Length; i++)
        {
            if (CellBtn[i] != null) // Null 체크 추가
            {
                CellBtn[i].interactable = (multiple + i < myList.Count) ? true : false;
                Text childText = CellBtn[i].transform.GetChild(0).GetComponent<Text>();
                if (childText != null)
                {
                    childText.text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
                }
            }
        }
    }

    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    Debug.Log("OnRoomListUpdate:" + roomList.Count);
    //    int roomCount = roomList.Count;
    //    for (int i = 0; i < roomCount; i++)
    //    {
    //        if (!roomList[i].RemovedFromList)
    //        {
    //            if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
    //            else myList[myList.IndexOf(roomList[i])] = roomList[i];
    //        }
    //        else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
    //    }

    //    MyListRenewal();

    //}

    public override void OnRoomListUpdate(List<RoomInfo> roomList)          //room갯수에 맞게 관리
    {
        Debug.Log("OnRoomListUpdate:" + roomList.Count);
        int roomCount = roomList.Count;

        // 모든 방 프리팹을 비활성화하여 초기화합니다.
        foreach (GameObject roomPrefab in roomPrefabsArray)
        {
            roomPrefab.SetActive(false);
        }

        // 방 목록을 UI에 업데이트합니다.
        for (int i = 0; i < roomCount && i < roomPrefabsArray.Length; i++)
        {
            RoomInfo roomInfo = roomList[i];
            GameObject roomPrefab = roomPrefabsArray[i];

            if (!roomInfo.RemovedFromList)
            {
                roomPrefab.SetActive(true);

                // 방 이름 업데이트
                Transform roomNameTransform = roomPrefab.transform.Find("RoomName");
                if (roomNameTransform != null)
                {
                    Text roomNameText = roomNameTransform.GetComponent<Text>();
                    if (roomNameText != null)
                    {
                        roomNameText.text = roomInfo.Name;
                    }
                }

                // 현재 인원 수 업데이트
                Transform headCountTransform = roomPrefab.transform.Find("HeadCount");
                if (headCountTransform != null)
                {
                    Text headCountText = headCountTransform.GetComponent<Text>();
                    if (headCountText != null)
                    {
                        headCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
                    }
                }
            }
        }

        MyListRenewal();
    }
    #endregion

}
