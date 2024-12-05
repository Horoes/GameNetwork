using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectUI : MonoBehaviourPunCallbacks
{
    public AbilityManager abilityManager;
    public List<Button> abilityButtons;

    private void Start()
    {
        if (abilityManager == null)
        {
            abilityManager = AbilityManager.Instance;
        }

        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void InitializeButtons()
    {
        Debug.Log($"Button Count: {abilityButtons.Count}, Current Abilities: {abilityManager.currentAbilities.Count}");

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i < abilityManager.currentAbilities.Count)
            {
                UpdateButtonUI(abilityButtons[i], abilityManager.currentAbilities[i]);
                int index = i; // 로컬 변수로 저장
                abilityButtons[i].onClick.RemoveAllListeners();
                abilityButtons[i].onClick.AddListener(() =>
                    abilityManager.SelectAbility(abilityManager.currentAbilities[index].abilityName, PhotonNetwork.IsMasterClient ? 1 : 2));
                abilityButtons[i].gameObject.SetActive(true);
            }
            else
            {
                abilityButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateButtonUI(Button button, Ability ability)
    {
        Text nameText = button.transform.Find("AbillityName")?.GetComponent<Text>();
        Text effectText = button.transform.Find("AbillityEffect")?.GetComponent<Text>();

        if (nameText != null) nameText.text = ability.abilityName;
        if (effectText != null) effectText.text = ability.abilityEffect;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1) // 초기 능력 데이터 수신
        {
            object[] data = (object[])photonEvent.CustomData;
            string[] abilityNames = (string[])data[0];
            string[] abilityEffects = (string[])data[1];

            abilityManager.currentAbilities.Clear();
            for (int i = 0; i < abilityNames.Length; i++)
            {
                Ability newAbility = new Ability
                {
                    abilityName = abilityNames[i],
                    abilityEffect = abilityEffects[i]
                };
                abilityManager.currentAbilities.Add(newAbility);
            }

            Debug.Log("초기 능력 데이터가 동기화되었습니다.");
            InitializeButtons();
        }
        else if (photonEvent.Code == 2) // 능력 선택 데이터 수신
        {
            object[] data = (object[])photonEvent.CustomData;
            string selectedAbilityName = (string)data[0];
            int playerID = (int)data[1];

            if (playerID == 1)
            {
                abilityManager.player1Abilities.Add(selectedAbilityName);
            }
            else if (playerID == 2)
            {
                abilityManager.player2Abilities.Add(selectedAbilityName);
            }

            abilityManager.currentAbilities.RemoveAll(a => a.abilityName == selectedAbilityName);
            Debug.Log($"플레이어 {playerID}가 능력 {selectedAbilityName}을 선택했습니다.");

            InitializeButtons();
        }
    }
}