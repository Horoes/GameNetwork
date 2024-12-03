using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectUI : MonoBehaviourPunCallbacks
{
    public AbilityManager abilityManager; // 능력 관리 매니저
    public List<Button> abilityButtons;  // 버튼 UI 리스트 (Inspector에서 설정)
    private List<Ability> selectedAbilities = new List<Ability>(); // 선택된 능력 저장

    private void Start()
    {
        // 방장이면 능력 초기화
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeButtons();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();

        // 비방장이면 방장에게 능력 데이터를 요청
        if (!PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RequestAbilityDataWithDelay());
        }
    }

    private IEnumerator RequestAbilityDataWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // 0.1초 지연 후 데이터 요청
        photonView.RPC("RequestAbilityData", RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RequestAbilityData()
    {
        // 방장이 currentAbilities 데이터를 다른 플레이어에게 전달
        List<string> abilityNames = new List<string>();
        List<string> abilityEffects = new List<string>();

        foreach (var ability in abilityManager.currentAbilities)
        {
            abilityNames.Add(ability.abilityName);
            abilityEffects.Add(ability.abilityEffect);
        }

        // 다른 플레이어에게 능력 데이터 동기화
        photonView.RPC("SyncAbilityData", RpcTarget.Others, abilityNames.ToArray(), abilityEffects.ToArray());
    }

    [PunRPC]
    private void SyncAbilityData(string[] abilityNames, string[] abilityEffects)
    {
        Debug.Log($"Syncing Ability Data: Names Count = {abilityNames.Length}, Effects Count = {abilityEffects.Length}");

        if (abilityNames.Length != abilityEffects.Length)
        {
            Debug.LogError("동기화된 데이터 크기가 일치하지 않습니다.");
            return;
        }

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

        Debug.Log("Current Abilities Updated Successfully");
        InitializeButtons(); // 동기화 후 버튼 초기화
    }

    private void InitializeButtons()
    {
        Debug.Log($"Ability Buttons Count: {abilityButtons.Count}, Current Abilities Count: {abilityManager.currentAbilities.Count}");
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i < abilityManager.currentAbilities.Count && abilityManager.currentAbilities[i] != null)
            {
                UpdateButtonUI(abilityButtons[i], abilityManager.currentAbilities[i]);
                abilityButtons[i].onClick.RemoveAllListeners();
                int index = i; // 로컬 변수로 i 저장
                abilityButtons[i].onClick.AddListener(() => OnAbilitySelected(abilityButtons[index], abilityManager.currentAbilities[index]));
            }
            else
            {
                Debug.Log($"Button {i} is being disabled as no matching ability exists.");
                abilityButtons[i].gameObject.SetActive(false); // 유효하지 않은 버튼은 비활성화
            }
        }
    }

    private void OnAbilitySelected(Button clickedButton, Ability selectedAbility)
    {
        Debug.Log($"Selected Ability: {selectedAbility.abilityName}");

        // 선택된 능력을 추가
        selectedAbilities.Add(selectedAbility);

        // 선택된 능력을 네트워크에 동기화
        photonView.RPC("SyncSelectedAbility", RpcTarget.All, selectedAbility.abilityName);

        // `currentAbilities`에서 선택된 능력을 제거
        abilityManager.currentAbilities.Remove(selectedAbility);

        // currentAbilities가 5개 이하일 때만 버튼 비활성화
        if (abilityManager.currentAbilities.Count <= 5)
        {
            GameObject parentButtonObject = clickedButton.transform.parent.gameObject;
            parentButtonObject.SetActive(false);
        }

        // 능력 리스트를 동기화하고 버튼을 업데이트
        SyncUpdatedAbilities();
    }

    [PunRPC]
    private void SyncSelectedAbility(string abilityName)
    {
        Ability selectedAbility = abilityManager.currentAbilities.Find(a => a.abilityName == abilityName);
        if (selectedAbility != null)
        {
            selectedAbilities.Add(selectedAbility);
            abilityManager.currentAbilities.Remove(selectedAbility);
        }
    }

    private void SyncUpdatedAbilities()
    {
        // 능력 리스트를 다른 클라이언트와 동기화
        List<string> abilityNames = new List<string>();
        List<string> abilityEffects = new List<string>();

        foreach (var ability in abilityManager.currentAbilities)
        {
            abilityNames.Add(ability.abilityName);
            abilityEffects.Add(ability.abilityEffect);
        }

        // 동기화 데이터를 전송
        photonView.RPC("SyncAbilityData", RpcTarget.All, abilityNames.ToArray(), abilityEffects.ToArray());

        // 현재 클라이언트에서 UI 갱신
        InitializeButtons();
    }

    private void UpdateButtonUI(Button button, Ability ability)
    {
        // 현재 Button의 부모(AbilitySelectBtn)를 기준으로 AbilityTextLayout 찾기
        Transform textLayoutTransform = button.transform;
        if (textLayoutTransform == null)
        {
            Debug.LogError($"'{button.name}' 버튼에서 AbilityTextLayout을 찾을 수 없습니다.");
            return;
        }

        Text abilityNameText = textLayoutTransform.Find("AbillityName")?.GetComponent<Text>();
        Text abilityEffectText = textLayoutTransform.Find("AbillityEffect")?.GetComponent<Text>();

        if (abilityNameText != null)
        {
            abilityNameText.text = ability.abilityName;
        }
        else
        {
            Debug.LogError($"AbilityName Text를 찾을 수 없습니다. ({button.name})");
        }

        if (abilityEffectText != null)
        {
            abilityEffectText.text = ability.abilityEffect;
        }
        else
        {
            Debug.LogError($"AbilityEffect Text를 찾을 수 없습니다. ({button.name})");
        }
    }
}