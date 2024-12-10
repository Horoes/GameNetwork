using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class AbilityManager : MonoBehaviourPunCallbacks
{
    public static AbilityManager Instance;

    public List<Ability> allAbilities; // 모든 능력 리스트
    public List<Ability> currentAbilities; // 현재 선택 가능한 능력 리스트
    public List<string> player1Abilities = new List<string>(); // Player 1의 선택된 능력
    public List<string> player2Abilities = new List<string>(); // Player 2의 선택된 능력

    public bool player1Ready = false; // 플레이어 1 준비 여부
    public bool player2Ready = false; // 플레이어 2 준비 여부
    public bool player1Won = false; // 플레이어 1 승리 여부
    public bool player2Won = false; // 플레이어 2 승리 여부

    public GameObject GameStartObject;

    private bool isCheckComplete = false;   // Ready변수들이 true일 때 작동

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeAbilities();
            SyncAbilitiesWithEvent();
        }

    }
    private void Update()
    {
        if (player1Ready && player2Ready&&!isCheckComplete)
        {
            isCheckComplete = true;
            CheckAllPlayersReady();
        }
    }
    public void InitializeAbilities()
    {
        currentAbilities = new List<Ability>(allAbilities);
        Debug.Log("능력 리스트가 초기화되었습니다.");
    }

    public void SyncAbilitiesWithEvent()
    {
        List<string> abilityNames = new List<string>();
        List<string> abilityEffects = new List<string>();

        foreach (var ability in currentAbilities)
        {
            abilityNames.Add(ability.abilityName);
            abilityEffects.Add(ability.abilityEffect);
        }

        object[] content = new object[] { abilityNames.ToArray(), abilityEffects.ToArray() };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, content, options, SendOptions.SendReliable);
    }


    public void SelectAbility(string abilityName, int playerID)
    {
        if (playerID == 1 && !player1Abilities.Contains(abilityName))
        {
            player1Abilities.Add(abilityName);
            player1Ready = true;
        }
        else if (playerID == 2 && !player2Abilities.Contains(abilityName))
        {
            player2Abilities.Add(abilityName);
            player2Ready = true;
        }

        currentAbilities.RemoveAll(a => a.abilityName == abilityName);

        // 이벤트로 선택된 능력을 동기화
        object[] content = new object[] { abilityName, playerID };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, content, options, SendOptions.SendReliable);

        Debug.Log($"플레이어 {playerID}가 능력 {abilityName}을 선택했습니다.");

        // UI 업데이트 호출
        UpdateAbilityUI();
    }
    private void UpdateAbilityUI()
    {
        // AbilitySelectUI 인스턴스를 찾아서 버튼 UI 갱신 호출
        AbilitySelectUI abilitySelectUI = FindObjectOfType<AbilitySelectUI>();
        if (abilitySelectUI != null)
        {
            abilitySelectUI.InitializeButtons();
        }
        else
        {
            Debug.LogWarning("AbilitySelectUI를 찾을 수 없습니다.");
        }
    }
    [PunRPC]
    public void UpdatePlayerReady(int playerID, string abilityName, bool isPlayer1Ready, bool isPlayer2Ready)
    {
        if (playerID == 1)
        {
            if (!player1Abilities.Contains(abilityName))
                player1Abilities.Add(abilityName);
            player1Ready = isPlayer1Ready;
        }
        else if (playerID == 2)
        {
            if (!player2Abilities.Contains(abilityName))
                player2Abilities.Add(abilityName);
            player2Ready = isPlayer2Ready;
        }

        Debug.Log($"RPC 수신: 플레이어 {playerID}가 능력 {abilityName}을 선택했습니다.");
        CheckAllPlayersReady();
    }
    public void CheckAllPlayersReady()
    {
        isCheckComplete = false;
     
        if (player1Ready && player2Ready) // 두 플레이어가 모두 준비 상태일 때만 실행
        {
            player1Ready = false;
            player2Ready = false;
            Debug.Log("모든 플레이어가 준비되었습니다. OnAllPlayersReady 호출");

            // 다음 씬 이동
            if (GameStartObject != null)
                GameStartObject.SetActive(true);
            StartCoroutine(LoadFirstMapScene()); // 3초 뒤 씬 이동
        }
        else
        {
            Debug.Log("아직 모든 플레이어가 준비되지 않았습니다.");
        }
    }
    private IEnumerator LoadFirstMapScene()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.LoadLevel("FirstMapScene");
    }
}
