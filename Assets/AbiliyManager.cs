using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviourPunCallbacks
{
    public List<Ability> allAbilities; // 모든 능력 리스트
    public List<Ability> currentAbilities; // 현재 선택 가능한 능력 리스트
 

    private void Start()
    {
        InitializeAbilities();
    }

    public void InitializeAbilities()
    {
        // 선택 가능한 능력 리스트 초기화
        currentAbilities = new List<Ability>(allAbilities);
    }

    public Ability GetRandomAbility()
    {
        if (currentAbilities.Count == 0)
        {
            Debug.LogWarning("선택 가능한 능력이 없습니다.");
            return null;
        }

        int randomIndex = Random.Range(0, currentAbilities.Count);
        Ability randomAbility = currentAbilities[randomIndex];
        currentAbilities.RemoveAt(randomIndex);
        return randomAbility;
    }
}