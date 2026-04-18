using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class PlayerStackHandler : MonoBehaviour
{
    [Header("Pivots")]
    public Transform backpackPivot_No1;
    public Transform backpackPivot_No2;
    public Transform frontHandPivot;

    [Header("Independent Capacities")]
    public int maxIronOre = 20;   // 철광석 최대치
    public int maxCash = 20;      // 돈 최대치
    public int maxHandcuff = 20;  // 수갑 최대치

    [Header("Settings")]
    [SerializeField] private float stepHeight = 0.2f;

    private List<ResourceItem> backStack1 = new List<ResourceItem>();
    private List<ResourceItem> backStack2 = new List<ResourceItem>();
    private List<ResourceItem> frontStack = new List<ResourceItem>();

    // [중요] 특정 자원 타입의 현재 개수만 합산해서 반환하는 함수
    public int GetCurrentCountByType(ResourceType type)
    {
        int current = 0; // 변수명을 명확히 함
        foreach (var item in backStack1)
        {
            if (item != null && item.type == type) current++;
        }
        foreach (var item in backStack2)
        {
            if (item != null && item.type == type) current++;
        }
        foreach (var item in frontStack)
        {
            if (item != null && item.type == type) current++;
        }
        return current;
    }

    // [수정] 타입별로 독립된 최대 용량과 비교합니다.
    public bool CanAdd(ResourceType type)
    {
        int current = GetCurrentCountByType(type);

        if (type == ResourceType.IronOre) return current < maxIronOre;
        if (type == ResourceType.Cash) return current < maxCash;
        if (type == ResourceType.Handcuff) return current < maxHandcuff;

        return false;
    }

    // 기존 Count 프로퍼티는 UI 표시용 등으로 유지 (전체 합계)
    public int Count => backStack1.Count + backStack2.Count + frontStack.Count;

    public void AddToStack(ResourceItem item, int dummy)
    {
        if (item == null || !CanAdd(item.type)) return;

        // [추가] 돈 아이템이 등에 쌓일 때 UI 점수를 즉시 올림
        if (item.type == ResourceType.Cash)
        {
            // GameManager에 돈 추가 (개당 10원 등 기획에 맞게 설정)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(10);
            }
        }

        Transform targetPivot;
        List<ResourceItem> targetList;

        if (item.type == ResourceType.Handcuff)
        {
            targetPivot = frontHandPivot;
            targetList = frontStack;
        }
        else
        {
            if (backStack1.Count == 0 || backStack1[0].type == item.type)
            {
                targetPivot = backpackPivot_No1;
                targetList = backStack1;
            }
            else
            {
                targetPivot = backpackPivot_No2;
                targetList = backStack2;
            }
        }

        // [핵심] 리스트에 추가하기 전의 Count로 정확한 Y좌표 계산
        Vector3 targetLocalPos = new Vector3(0, targetList.Count * stepHeight, 0);

        // 리스트에 추가
        targetList.Add(item);

        // 점프 실행 (시작하자마자 부모를 설정하도록 수정된 JumpTo 호출)
        item.JumpTo(targetPivot, targetLocalPos, 0.25f);
    }

    public ResourceItem RemoveSpecificType(ResourceType targetType)
    {
        // 1. 수갑 우선 체크
        if (targetType == ResourceType.Handcuff && frontStack.Count > 0)
        {
            ResourceItem item = frontStack[frontStack.Count - 1];
            frontStack.RemoveAt(frontStack.Count - 1);
            return item;
        }

        // 2. 백팩 줄 확인 (철광석이나 돈)
        for (int i = backStack2.Count - 1; i >= 0; i--)
        {
            if (backStack2[i].type == targetType)
            {
                ResourceItem item = backStack2[i];
                backStack2.RemoveAt(i);
                Reorganize(backStack2, backpackPivot_No2); // [추가] 정렬 실행
                return item;
            }
        }

        for (int i = backStack1.Count - 1; i >= 0; i--)
        {
            if (backStack1[i].type == targetType)
            {
                ResourceItem item = backStack1[i];
                backStack1.RemoveAt(i);

                if (backStack1.Count == 0 && backStack2.Count > 0)
                    ShiftStack2ToStack1();
                else
                    Reorganize(backStack1, backpackPivot_No1); // [추가] 정렬 실행

                return item;
            }
        }
        return null;
    }

    private void Reorganize(List<ResourceItem> list, Transform pivot)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector3 targetPos = new Vector3(0, i * stepHeight, 0);
            list[i].JumpTo(pivot, targetPos, 0.2f);
        }
    }

    private void ShiftStack2ToStack1()
    {
        foreach (var item in backStack2)
        {
            backStack1.Add(item);
            Vector3 newPos = new Vector3(0, (backStack1.Count - 1) * stepHeight, 0);
            item.JumpTo(backpackPivot_No1, newPos, 0.2f);
        }
        backStack2.Clear();
    }

    public bool HasResourceType(ResourceType type) => GetCurrentCountByType(type) > 0;
}