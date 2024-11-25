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
    bool isReady = false;
    string playerName = "";
    string connectionState = "";
    public string chatMessage;
    Text chatText;
    ScrollRect scroll_rect = null;
    PhotonView pv;

    Text connectionInfoText;

    [Header("LobbyCanvas")] public GameObject LobbyCanvas;
     public GameObject[] roomPrefabsArray; // Room 프리팹들
    public GameObject LoginPopup;
    public GameObject MakeRoomPanel;
    public GameObject LobbyPopup;
    public GameObject RoomPopup;
    public InputField RoomInput;
    public InputField RoomPwInput;
    public Text WaitText;
    public Text GameStartText;
    public Text RoomNameTitle;
    public Text HostPlayer;
    public Text VisitPlayer;
    public Text VisitEmptyName;
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
    public MakingRoom makingRoom;
  
    public int hashtablecount;

    public int currentRoomIndex;
    private int[] roomCounts;
    // �������� �� ����Ʈ�� �����ϴ� ����
    List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple, roomnumber;

    private void Awake()
    {

        currentRoomIndex = 0;
        PhotonNetwork.GameVersion = "MyFps 1.0";
        PhotonNetwork.ConnectUsingSettings();

        // ���� 2���� ������ �ε��� �Ǳ⶧���� UI ó���� �غ���
        if (GameObject.Find("ChatText") != null)
            chatText = GameObject.Find("ChatText").GetComponent<Text>();

        if (GameObject.Find("Scroll View") != null)
            scroll_rect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();

        if (GameObject.Find("ConnectionInfoText") != null)
            connectionInfoText = GameObject.Find("ConnectionInfoText").GetComponent<Text>();

        connectionState = "마스터 서버에 접속 중...";

        if (connectionInfoText)
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
            if (!instance)
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
        Debug.Log("로비 입장");
    }
    public void RoomGotoLobby()
    {
      
        string roomName = PhotonNetwork.CurrentRoom.Name;
        // RoomIndex를 이용해 관련 Room 오브젝트를 찾기
        int roomIndex = GetRoomIndexByName(roomName);
        Transform roomTransform = roomPrefabsArray[roomIndex].transform;

        // 텍스트 초기화
        ClearRoomUI(roomTransform);


        Debug.Log("방을 나왔습니다.");
        PhotonNetwork.LeaveRoom();                  // 방을 떠났을 때
       
        
        RoomPopup.SetActive(false);
        LobbyPopup.SetActive(true);
        
        PhotonNetwork.JoinLobby();
    }
    // Room UI 텍스트 초기화 함수
    private void ClearRoomUI(Transform roomTransform)
    {
        // RoomName 텍스트 비우기
        Text roomNameText = roomTransform.Find("RoomName").GetComponent<Text>();
        if (roomNameText != null) roomNameText.text = "";

        // HeadCount 텍스트 비우기
        Text headCountText = roomTransform.Find("HeadCount").GetComponent<Text>();
        if (headCountText != null) headCountText.text = "";
        roomTransform.gameObject.SetActive(false);

        UpdateRoomCountUI();
    }
    public void UpdateRoomCountUI()
    {
        int activeRoomCount = 0;

        // 활성화된 Room 오브젝트 개수 계산
        foreach (GameObject roomPrefab in roomPrefabsArray)
        {
            if (roomPrefab.activeSelf) // 활성화된 방만 카운트
            {
                activeRoomCount++;
            }
        }

        // UI 업데이트 (예: Text 컴포넌트에 표시)
        if (makingRoom != null && makingRoom.roomCount != null)
        {
            makingRoom.roomCount.text = $"{activeRoomCount}/7"; // 7은 최대 방 개수
        }

    
    }
    private int GetRoomIndexByName(string roomName)     // 방 이름을 기준으로 생성된 방들의 인덱스를 찾는 함수
    {
        for (int i = 0; i < roomPrefabsArray.Length; i++)
        {
            Transform roomNameTransform = roomPrefabsArray[i].transform.Find("RoomName");
            if (roomNameTransform != null)
            {
                Text roomNameText = roomNameTransform.GetComponent<Text>();
                if (roomNameText != null && roomNameText.text == roomName)
                {
                    return i; // 인덱스 반환
                }
            }
        }
        return -1; // 찾지 못한 경우 -1 반환
    }
    
    public override void OnPlayerLeftRoom(Photon.Realtime.Player newPlayer)
    {
       
        if (VisitEmptyName != null)
        {
            VisitPlayer.gameObject.SetActive(false);
            VisitEmptyName.gameObject.SetActive(true);
        }
        WaitText.gameObject.SetActive(true);
        GameStartText.gameObject.SetActive(false);
        Debug.Log("플레이어가 방을 떠났습니다: " + newPlayer.NickName);
        //OnPlayerLeave(currentRoomIndex - 1);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName}님이 방에 입장했습니다.");
        Debug.Log($"현재 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        // 다른 플레이어가 입장 시 방문자 Text ui 업데이트
        if (VisitEmptyName != null)
        {
            VisitPlayer.text = newPlayer.NickName; // 입장한 플레이어의 닉네임 표시
            VisitEmptyName.gameObject.SetActive(false);
            VisitPlayer.gameObject.SetActive(true);
        }
        // 방 정원이 모두 찼으면 게임 시작
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            WaitText.gameObject.SetActive(false);
            GameStartText.gameObject.SetActive(true);
            //StartGame();
        }
    }

    public void Connect()
    {
        PhotonNetwork.NickName=playerInput.text;
        Debug.Log("Connect 성공!");
        if (PhotonNetwork.IsConnected)
        {
            connectionState = "연결 중...";
            if (connectionInfoText)
                connectionInfoText.text = connectionState;

            LoginPopup.SetActive(false);
            LobbyPopup.SetActive(true);

            PhotonNetwork.JoinLobby();

        }
        else
        {
            connectionState = "연결 실패 : 서버에 접속하지 못했습니다\n재접속 시도 중...";
            if (connectionInfoText)
                connectionInfoText.text = connectionState;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnCreatedRoom()
    {
        UpdateRoomCountUI();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("OnCreateRoomFailed:" + returnCode + "-" + message);
    }

    public void JoinRoom(string roomName)
    {
        if(PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogError("방 입장 실패");
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        connectionState = "Joined Room";
        if (connectionInfoText)
            connectionInfoText.text = connectionState;
        Debug.Log("방에 입장했습니다.");
        isLoggIn = true;

        GameObject createBtn = GameObject.Find("CreateBtn");

      
        UpdatePlayerCount(makingRoom.currentRoomIndex);
        makingRoom.currentRoomIndex++;

        
        LobbyPopup.SetActive(false);
        RoomPopup.SetActive(true);

        HostPlayer.text = PhotonNetwork.NickName;

        string currentRoomName = PhotonNetwork.CurrentRoom.Name;

        if(RoomNameTitle!=null)
        {
            RoomNameTitle.text= currentRoomName;
        }

        Debug.Log($"방 이름: {RoomNameTitle}");


        //PlayerPrefs.SetInt("LogIn", 1);
        //PlayerPrefs.SetString("Player", playerName);


        //SceneManager.LoadScene("SampleScene");
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
        if (isGameStart == false && SceneManager.GetActiveScene().name == "SampleScene" && isLoggIn == true)        // 현재 씬이 SampleScene이고 boo 변수 조건 맞음 게임 스타트 한 후 플레이어 생성
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
        while (!isGameStart)
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
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 입장");
        isReady = true;
    }
    private void OnGUI()
    {
        GUILayout.Label(connectionState);
    }

    public void SetPlayerName()               // JoinBtn에 OnClick으로 연결되있는 SetPlayerName을 Connect로 수정
    {
        Debug.Log(playerInput.text + "님이 입력하였습니다!");

        if (isGameStart == false && isLoggIn == false)
        {
            playerName = playerInput.text;

            PhotonNetwork.NickName = playerName;

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

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        LobbyPopup.SetActive(false);
        LoginPopup.SetActive(true);
        connectionState = "������ ������ ���� ��...";
        if (connectionInfoText)
            connectionInfoText.text = connectionState;
        isGameStart = false;
        isLoggIn = false;
        PlayerPrefs.SetInt("LogIn", 0);

    }
    public void UpdatePlayerCount(int roomIndex)     // 생성된 방 인원 Text ui
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

    
    //public void CreateRoom()
    //{
    //     PhotonNetwork.CreateRoom(RoomInput.text == "" ? "Game" + Random.Range(0, 100) : RoomInput.text,
    //            new RoomOptions { MaxPlayers = 20 });
    //    LobbyPanel.SetActive(false);
    //}

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
    //public void MyListClick(int num)
    //{

    //    if (num == -2)
    //    {
    //        --currentPage;
    //        MyListRenewal();
    //    }
    //    else if (num == -1)
    //    {
    //        ++currentPage;
    //        MyListRenewal();
    //    }

    //    else if (myList[multiple + num].CustomProperties["password"] != null)
    //    {
    //        PwPanel.SetActive(true);
    //    }
    //    else
    //    {
    //        PhotonNetwork.JoinRoom(myList[multiple + num].Name);
    //        MyListRenewal();

    //    }

    //}

    //public void RoomPw(int number)
    //{
    //    switch (number)
    //    {
    //        case 0:
    //            roomnumber = 0;
    //            break;
    //        case 1:
    //            roomnumber = 1;
    //            break;
    //        case 2:
    //            roomnumber = 2;
    //            break;
    //        case 3:
    //            roomnumber = 3;
    //            break;

    //        default:
    //            break;
    //    }


    //}

    //public void EnterRoomWithPW()
    //{
    //    if ((string)myList[multiple + roomnumber].CustomProperties["password"] == PwCheckIF.text)
    //    {
    //        PhotonNetwork.JoinRoom(myList[multiple + roomnumber].Name);
    //        MyListRenewal();
    //        PwPanel.SetActive(false);
    //    }

    //    else
    //    {
    //        StartCoroutine("ShowPwWrongMsg");
    //    }


    //}

    //IEnumerator ShowPwWrongMsg()
    //{
    //    if (!PwErrorLog.activeSelf)
    //    {
    //        PwErrorLog.SetActive(true);
    //        yield return new WaitForSeconds(3.0f);
    //        PwErrorLog.SetActive(false);
    //    }
    //}

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
    //    Debug.Log("OnRoomListUpdate 호출됨: 방 목록 변경 감지");

    //    foreach (RoomInfo room in roomList)
    //    {
    //        if (room.RemovedFromList) // 방이 삭제된 경우
    //        {
    //            Debug.Log($"방 삭제됨: {room.Name}");
    //            DeactivateRoomUI(room.Name); // 방 UI 비활성화
    //        }
    //        else // 방이 추가 또는 갱신된 경우
    //        {
    //            Debug.Log($"방 추가/갱신됨: {room.Name}");
    //            ActivateRoomUI(room.Name, room.PlayerCount, room.MaxPlayers); // 방 UI 활성화 및 업데이트
    //        }
    //    }
    //}
    //private void DeactivateRoomUI(string roomName)
    //{
    //    // RoomIndex를 통해 오브젝트 가져오기
    //    int roomIndex = GetRoomIndexByName(roomName);
    //    Transform roomTransform = roomPrefabsArray[roomIndex].transform;

    //    // Room 오브젝트 비활성화
    //    roomTransform.gameObject.SetActive(false);

    //    Debug.Log($"방 UI 비활성화: {roomName}");
    //}
    //private void ActivateRoomUI(string roomName, int currentPlayers, int maxPlayers)
    //{
    //    // RoomIndex를 통해 오브젝트 가져오기
    //    int roomIndex = GetRoomIndexByName(roomName);
    //    Transform roomTransform = roomPrefabsArray[roomIndex].transform;

    //    // Room 오브젝트 활성화
    //    roomTransform.gameObject.SetActive(true);

    //    // UI 업데이트
    //    Text roomNameText = roomTransform.Find("RoomName").GetComponent<Text>();
    //    if (roomNameText != null) roomNameText.text = roomName;

    //    Text headCountText = roomTransform.Find("HeadCount").GetComponent<Text>();
    //    if (headCountText != null) headCountText.text = $"{currentPlayers}/{maxPlayers}";

    //    Debug.Log($"방 UI 활성화: {roomName} ({currentPlayers}/{maxPlayers})");
    //}
    //public override void OnRoomListUpdate(List<RoomInfo> roomList)          //room갯수에 맞게 관리
    //{
    //    Debug.Log("OnRoomListUpdate:" + roomList.Count);
    //    int roomCount = roomList.Count;

    //    // 모든 방 프리팹을 비활성화하여 초기화합니다.
    //    foreach (GameObject roomPrefab in roomPrefabsArray)
    //    {
    //        roomPrefab.SetActive(false);
    //    }

    //    // 방 목록을 UI에 업데이트합니다.
    //    for (int i = 0; i < roomCount && i < roomPrefabsArray.Length; i++)
    //    {
    //        RoomInfo roomInfo = roomList[i];
    //        GameObject roomPrefab = roomPrefabsArray[i];

    //        if (!roomInfo.RemovedFromList)
    //        {
    //            roomPrefab.SetActive(true);

    //            // 방 이름 업데이트
    //            Transform roomNameTransform = roomPrefab.transform.Find("RoomName");
    //            if (roomNameTransform != null)
    //            {
    //                Text roomNameText = roomNameTransform.GetComponent<Text>();
    //                if (roomNameText != null)
    //                {
    //                    roomNameText.text = roomInfo.Name;
    //                }
    //            }

    //            // 현재 인원 수 업데이트
    //            Transform headCountTransform = roomPrefab.transform.Find("HeadCount");
    //            if (headCountTransform != null)
    //            {
    //                Text headCountText = headCountTransform.GetComponent<Text>();
    //                if (headCountText != null)
    //                {
    //                    headCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    //                }
    //            }
    //        }
    //    }

    //    MyListRenewal();
    //}

    //public override void OnJoinedLobby()
    //{
    //    base.OnJoinedLobby();
    //    connectionState = "Joined Lobby";
    //    if (connectionInfoText)
    //        connectionInfoText.text = connectionState;
    //    myList.Clear();

    //    Debug.Log("OnJoinedLobby:" );

    //}

    //public override void OnJoinRandomFailed(short returnCode, string message)     // 랜덤으로 생성한 방이 없어 입장하지 못하면 해당 콜백함수 호출
    //{
    //    connectionState = "No Room";
    //    if (connectionInfoText)
    //        connectionInfoText.text = connectionState;
    //    Debug.Log("No Room");
    //    //����� �κе� �ּ� ó��
    //    //PhotonNetwork.CreateRoom("MyRoom");
    //}
    #endregion


}
