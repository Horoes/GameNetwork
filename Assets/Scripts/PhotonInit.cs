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
    public GameObject FullRoomImage;
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
    public Button RoomExitBtn;
    public bool LockState = false;
    public string privateroom;
    public Button[] CellBtn;
    //public Button PreviousBtn;
    //public Button NextBtn;
    public Button CreateRoomBtn;
    public MakingRoom makingRoom;

    public int hashtablecount;

    public int currentRoomIndex;
    private int[] roomCounts;

    // 플레이어 점수 관리
    public GameObject gameEndPopup;
    public GameObject roundEndPopup;        // 라운드 종료 Popup 이미지 UI
    public GameObject[] playerRoundWins;    // 라운드 승리   UI
    public GameObject[] playerWinIcons;     // 점수 아이콘   UI
    public GameObject[] playerGameWins;     // 플레이어 최종 승리 UI
    public int[] playerScores = new int[2];
    public int winningScore = 3;

    public bool isRoundActive = false;

    // �������� �� ����Ʈ�� �����ϴ� ����
    public List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple, roomnumber;

    public void EndRound(int winningPlayerIndex)    // 라운드 종료 시 실행
    {
        if (isRoundActive)
            return;
        isRoundActive = true;

        // 점수 업데이트
        playerScores[winningPlayerIndex]++;
        Debug.Log($"Player {winningPlayerIndex + 1} 점수: {playerScores[winningPlayerIndex]}");

        StartCoroutine(ShowRoundEndUI(winningPlayerIndex));

    }

    private IEnumerator ShowRoundEndUI(int winningPlayerIndex)  // 라운드 종료 시 점수 업데이트 
    {
        // RoundEndPopup 활성화 (라운드 승리 UI)
        GameObject roundEndPopup = GameObject.Find("RoundEndPopup");
        GameObject winnerUI = roundEndPopup.transform.Find($"P{winningPlayerIndex + 1}_RoundWin").gameObject;

        roundEndPopup.SetActive(true);
        winnerUI.SetActive(true);

        yield return new WaitForSeconds(2f);

        // 점수 아이콘 업데이트(활성화)
        GameObject scoreLayout = GameObject.Find($"P{winningPlayerIndex + 1}_ScoreLayout");
        GameObject winIcon = scoreLayout.transform.Find($"WinIcon").gameObject;
        winIcon.SetActive(true);

        yield return new WaitForSeconds(2f);

        // UI 초기화
        winnerUI.SetActive(false);
        roundEndPopup.SetActive(false);



        if (playerScores[winningPlayerIndex] >= winningScore)
        {
            HandleGameEnd(winningPlayerIndex);
        }
        else
        {
            // 다음 라운드 준비 (AbilitySelectScene으로 이동)
            PhotonNetwork.LoadLevel("AbilitySelectScene");
            isRoundActive = false;
        }

    }

    private void HandleGameEnd(int winningPlayerIndex)
    {
        // 최종 승리 UI 활성화
        gameEndPopup.SetActive(true);
        playerGameWins[winningPlayerIndex].SetActive(true);

        StartCoroutine(HandleGameEndUI());
    }

    private IEnumerator HandleGameEndUI()
    {
        yield return new WaitForSeconds(2f);

        // 점수 초기화
        ResetGame();

        // 로비로 돌아가기
        PhotonNetwork.LoadLevel("LobbyScene");


    }

    private void ResetGame()            // UI들 초기화
    {
        playerScores = new int[2];
        isRoundActive = false;

        // 점수 UI 초기화
        foreach (var winIcon in playerWinIcons)
        {
            winIcon.SetActive(false);
        }

        // 라운드 및 게임 종료 UI 초기화
        foreach (var roundWin in playerRoundWins)
        {
            roundWin.SetActive(false);
        }

        gameEndPopup.SetActive(false);
        foreach (var gameWin in playerGameWins)
        {
            gameWin.SetActive(false);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentRoomIndex = 0;
        PhotonNetwork.GameVersion = "MyFps 1.0";
        PhotonNetwork.ConnectUsingSettings();

        if (PhotonNetwork.IsConnected)
            Debug.Log("포톤 네트워크 연결 확인");

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

        DontDestroyOnLoad(gameObject);
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
        //LobbyPopup.SetActive(true);

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

    public override void OnPlayerLeftRoom(Photon.Realtime.Player newPlayer)     // 방에 들어온 플레이어가 방을 나갔을 때
    {

        if (VisitEmptyName != null)
        {
            VisitPlayer.gameObject.SetActive(false);        // 방문자 이름 off
            VisitEmptyName.gameObject.SetActive(true);      // 이름 내용: "비어있음"
        }
        if (!RoomExitBtn.gameObject.activeSelf)
        {
            RoomExitBtn.gameObject.SetActive(true);
        }
        WaitText.gameObject.SetActive(true);
        GameStartText.gameObject.SetActive(false);
        Debug.Log("플레이어가 방을 떠났습니다: " + newPlayer.NickName);
        //OnPlayerLeave(currentRoomIndex - 1);

        // 방이 비었는지 확인하고 UI 업데이트
        bool isAnyRoomAvailable = myList.Exists(room => room.PlayerCount < room.MaxPlayers);
        UpdateFullRoomUI();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName}님이 방에 입장했습니다.");
        Debug.Log($"현재 플레이어 수: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");

        // 방장이 방문자 TEXT UI를 업데이트 해준다.
        // 방문자 텍스트 UI 업데이트
        if (VisitEmptyName != null && VisitPlayer != null)
        {
            VisitPlayer.text = newPlayer.NickName; // 입장한 플레이어의 닉네임 표시
            VisitEmptyName.gameObject.SetActive(false);
            VisitPlayer.gameObject.SetActive(true);
        }

        // 게임 시작 UI 트리거 조건
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            WaitText.gameObject.SetActive(false);
            GameStartText.gameObject.SetActive(true);
            RoomExitBtn.gameObject.SetActive(false);
            //isGameStart = true;
            StartCoroutine(LoadAbilitySelectScene());
            // 씬 로드
        }
        else
        {
            // 방이 아직 꽉 차지 않은 경우 대기 텍스트 표시
            WaitText.gameObject.SetActive(true);
            GameStartText.gameObject.SetActive(false);
        }

        // 방 상태 다시 확인
        bool isAnyRoomAvailable = myList.Exists(room => room.PlayerCount < room.MaxPlayers);
        UpdateFullRoomUI();
    }


    public void Connect()
    {

        PhotonNetwork.NickName = playerInput.text;
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
        Debug.Log("방이 생성되었습니다.");
        UpdateRoomCountUI();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("OnCreateRoomFailed:" + returnCode + "-" + message);
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.InLobby) // 로비에 입장한 상태인지 확인
        {
            PhotonNetwork.JoinRoom(roomName);
            Debug.Log($"Joining room: {roomName}");
        }
        else
        {
            Debug.LogWarning("현재 로비에 입장하지 않은 상태입니다. JoinRoom을 호출할 수 없습니다.");
        }
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("로비에 성공적으로 입장했습니다.");
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

        RoomPopup.SetActive(true);

        // 방 제목 설정
        string currentRoomName = PhotonNetwork.CurrentRoom.Name;
        if (RoomNameTitle != null)
        {
            RoomNameTitle.text = currentRoomName;
        }

        // 방장의 닉네임 가져오기
        string hostPlayerName = null;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player.IsMasterClient) // 방장 확인
            {
                hostPlayerName = player.NickName;
                break;
            }
        }

        // 방장 이름 UI 설정
        if (HostPlayer != null)
        {
            HostPlayer.text = hostPlayerName ?? "알 수 없음";
        }

        //// 현재 플레이어가 입장한 닉네임을 방문자로 표시
        //if (VisitEmptyName != null && VisitPlayer != null)
        //{
        //    VisitPlayer.text = PhotonNetwork.LocalPlayer.NickName; // 현재 플레이어의 닉네임
        //    VisitEmptyName.gameObject.SetActive(false);
        //    VisitPlayer.gameObject.SetActive(true);
        //}

        // 방이 꽉 찼을 때 게임 시작 조건 체크
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            WaitText.gameObject.SetActive(false);       // 대기 문구 숨김
            GameStartText.gameObject.SetActive(true);   // 게임 시작 문구 표시
            RoomExitBtn.gameObject.SetActive(false);    // 나가기 버튼 비활성화
            isGameStart = true;

            Debug.Log("방이 꽉 찼습니다. 게임을 시작합니다!");

            // 씬 로드
            StartCoroutine(LoadAbilitySelectScene());
        }
        else
        {
            // 방이 아직 꽉 차지 않았으면 대기 문구 표시
            WaitText.gameObject.SetActive(true);
            GameStartText.gameObject.SetActive(false);
        }
    }
    private IEnumerator LoadAbilitySelectScene()
    {
        isRoundActive = false;
        yield return new WaitForSeconds(2f); // 2초 대기
        PhotonNetwork.LoadLevel("AbilitySelectScene"); // AbilitySelectScene으로 이동
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
        if (SceneManager.GetActiveScene().name == "BattleScene" && !isGameStart && isLoggIn)
        {
            isGameStart = true;
            StartCoroutine(CreatePlayer()); // 플레이어 생성 코루틴 실행
        }
    }

    IEnumerator CreatePlayer()
    {
        while (!isGameStart)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 정해진 스폰 위치 설정
        GameObject spawnPoint = PhotonNetwork.IsMasterClient
          ? GameObject.Find("SpawnPoint1") // MasterClient가 사용하는 스폰 포인트
          : GameObject.Find("SpawnPoint2"); // Non-MasterClient가 사용하는 스폰 포인트

        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint를 찾을 수 없습니다!");
            yield break;
        }

        // UI의 위치 가져오기
        RectTransform spawnRect = spawnPoint.GetComponent<RectTransform>();
        if (spawnRect == null)
        {
            Debug.LogError("SpawnPoint에 RectTransform 컴포넌트가 없습니다!");
            yield break;
        }

        Vector3 spawnPosition = spawnRect.anchoredPosition;

        // 플레이어 인덱스에 따라 스폰 위치를 설정
        string prefabName = PhotonNetwork.IsMasterClient ? "Player1" : "Player2"; // 마스터 클라이언트는 0, 다른 클라이언트는 1

        // Instantiate 및 캔버스 자식으로 설정
        GameObject player = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
        player.transform.SetParent(GameObject.Find("Canvas").transform, false);    // 생성된 오브젝트를 Canvas 자식으로 설정

        // 생성된 플레이어의 RectTransform 위치 설정
        RectTransform playerRect = player.GetComponent<RectTransform>();
        playerRect.anchoredPosition = spawnPosition;

        Debug.Log($"Player {prefabName} spawned at {spawnPosition}");

        // 플레이어 이름 설정
        playerRect.GetComponent<PlayerCtrl>().SetPlayerName(playerName);

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

    void MyListRenewal()
    {
        Debug.Log("Renewing list...");
        maxPage = (myList.Count % CellBtn.Length == 0)
                  ? myList.Count / CellBtn.Length
                  : myList.Count / CellBtn.Length + 1;

        multiple = (currentPage - 1) * CellBtn.Length;

        for (int i = 0; i < CellBtn.Length; i++)
        {
            if (multiple + i < myList.Count)
            {
                RoomInfo roomInfo = myList[multiple + i];
                Debug.Log($"{myList.Count}");
                // 버튼 활성화
                CellBtn[i].gameObject.SetActive(true);
                CellBtn[i].interactable = true;

                // 버튼 텍스트 설정
                Text roomNameText = CellBtn[i].transform.GetChild(0).GetComponent<Text>();
                if (roomNameText != null)
                {
                    roomNameText.text = roomInfo.Name;
                }

                // 클릭 이벤트 추가 (이전에 설정된 리스너 제거)
                CellBtn[i].onClick.RemoveAllListeners();
                CellBtn[i].onClick.AddListener(() => JoinRoom(roomInfo.Name));
            }
            else
            {
                // 버튼 비활성화
                CellBtn[i].gameObject.SetActive(false);
            }
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"OnRoomListUpdate: {roomList.Count} rooms updated.");
        int activeRoomCount = 0;
        foreach (var room in roomList)
        {
            Debug.Log($"Room: {room.Name}, Removed: {room.RemovedFromList}, Players: {room.PlayerCount}/{room.MaxPlayers}");
        }

        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1)
            {
                myList.RemoveAt(myList.IndexOf(roomList[i]));
            }

            if (roomList[i].PlayerCount == roomList[i].MaxPlayers)
            {
                activeRoomCount++;
            }
        }
        Debug.Log($"OnRoomListUpdate: {roomList.Count} rooms updated.");
        foreach (var room in roomList)
        {
            Debug.Log($"Room: {room.Name}, Removed: {room.RemovedFromList}, Players: {room.PlayerCount}/{room.MaxPlayers}");
        }

        UpdateFullRoomUI();

        MyListRenewal();
    }

    private void UpdateFullRoomUI()
    {
        int activeRoomCount = 0; // 활성화된 방 개수를 세는 변수

        foreach (GameObject roomPrefab in roomPrefabsArray)
        {
            if (roomPrefab.activeSelf) // 활성화된 방인지 확인
            {
                activeRoomCount++;
            }
        }

        // 활성화된 방의 개수가 roomPrefabsArray 길이와 같다면 모든 방이 활성화된 상태
        if (activeRoomCount == 7)
        {
            FullRoomImage.SetActive(true);
            Debug.Log($"모든 방이 활성화되었습니다! FullRoomImage 활성화. (활성화된 방 개수: {activeRoomCount})");
        }
        else
        {
            FullRoomImage.SetActive(false);
            Debug.Log($"아직 모든 방이 활성화되지 않았습니다. FullRoomImage 비활성화. (활성화된 방 개수: {activeRoomCount})");
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
    //    foreach (var room in roomList)
    //    {
    //        Debug.Log($"Room: {room.Name}, Players: {room.PlayerCount}/{room.MaxPlayers}");
    //    }
    //    MyListRenewal();

    //}
    #endregion


}
