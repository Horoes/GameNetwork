using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviourPunCallbacks
{
    public static AbilityManager Instance;

    public List<Ability> allAbilities; // 모든 능력 리스트
    public List<Ability> currentAbilities; // 현재 선택 가능한 능력 리스트
    public List<string> player1Abilities = new List<string>(); // Player 1의 선택된 능력
    public List<string> player2Abilities = new List<string>(); // Player 2의 선택된 능력

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
        // 선택된 능력 처리
        if (playerID == 1)
        {
            player1Abilities.Add(abilityName);
        }
        else if (playerID == 2)
        {
            player2Abilities.Add(abilityName);
        }

        currentAbilities.RemoveAll(a => a.abilityName == abilityName); // 선택된 능력 제거

        // 선택된 능력을 동기화
        object[] content = new object[] { abilityName, playerID };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, content, options, SendOptions.SendReliable);
    }
}