using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;
using static Interfaces;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerStackHandler playerStack;
    public GameObject orePrefab;

    [Header("UI Feedback")]
    public GameObject maxTextPrefab;

    public EquipmentDataSO currentEqData;

    private IMineable currentTargetRock;
    private bool isMining = false; // 이 플래그가 false가 되는 순간 다음 채굴 시작

    private readonly int doMiningHash = Animator.StringToHash("doMining");
    private readonly int doDrillingHash = Animator.StringToHash("doDrilling");

    public void UpdateEquipmentData(EquipmentDataSO data)
    {
        currentEqData = data;

        if (playerStack != null)
        {
            // 1. 철광석 수용량 갱신
            playerStack.maxIronOre = data.maxCapacity;

            // 2. 돈 수용량 갱신 (철광석과 동일하게 늘리거나, 별도 비율을 줄 수도 있습니다)
            playerStack.maxCash = data.maxCapacity;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("IronStone"))
        {
            currentTargetRock = other.GetComponent<IMineable>();

            if (currentTargetRock != null && currentTargetRock.IsActive)
            {
                // [수정] 시간 제한(lastMineTime) 없이 애니메이션만 끝났다면 바로 채굴
                if (!isMining)
                {
                    PerformMining();
                }
            }
        }

        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact(this);
        }
    }

    private void PerformMining()
    {
        isMining = true;

        Animator anim = GetComponent<Animator>();

        if (currentEqData != null && currentEqData.isDrillType)
        {
            anim.SetTrigger(doDrillingHash);
        }
        else
        {
            anim.SetTrigger(doMiningHash);
        }
    }

    public void OnMiningImpact()
    {
        // --- 드릴/불도저 타입 (다중 센서 대응) ---
        if (currentEqData != null && currentEqData.isDrillType)
        {
            // 1. 모든 자식에서 DrillHead 컴포넌트들을 다 가져옵니다.
            DrillHead[] allDrillHeads = GetComponentsInChildren<DrillHead>();

            if (allDrillHeads.Length > 0)
            {
                // 2. 중복 타격 방지를 위해 HashSet 사용 (여러 드릴이 한 바위 칠 때 대비)
                HashSet<IMineable> uniqueRocks = new HashSet<IMineable>();

                foreach (var head in allDrillHeads)
                {
                    head.CleanUpList(); // 죽은 바위 정리
                    foreach (var rock in head.touchingRocks)
                    {
                        uniqueRocks.Add(rock); // 중복되지 않게 담기
                    }
                }

                // 3. 수집된 모든 고유 광석들 채굴 처리
                foreach (var rock in uniqueRocks)
                {
                    ProcessMining(rock);
                }
                return;
            }
        }

        // --- 일반 곡괭이 타입 (단일 채굴) ---
        if (currentTargetRock != null && currentTargetRock.IsActive)
        {
            ProcessMining(currentTargetRock);
        }
    }

    // 실제 채굴 로직을 별도 함수로 분리 (중복 방지)
    private void ProcessMining(IMineable target)
    {
        if (target == null || !target.IsActive) return;

        // 가방 체크
        if (!playerStack.CanAdd(ResourceType.IronOre))
        {
            ShowMaxFeedback(((Component)target).transform.position + Vector3.up * 2f);
            return;
        }

        // 데미지 및 보상 생성
        target.TakeDamage(currentEqData.mineDamage, ((Component)target).transform.position);

        GameObject oreObj = ObjectPool.Instance.Pop(orePrefab, ((Component)target).transform.position, Quaternion.identity);
        ResourceItem item = oreObj.GetComponent<ResourceItem>();

        if (item != null)
        {
            playerStack.AddToStack(item, currentEqData.maxCapacity);
        }
    }

    private void ShowMaxFeedback(Vector3 position)
    {
        if (maxTextPrefab != null)
        {
            ObjectPool.Instance.Pop(maxTextPrefab, position, Quaternion.identity);
        }
    }

    // 애니메이션 이벤트: 채굴 동작이 완전히 끝났을 때 호출
    public void FinishMining()
    {
        isMining = false;
    }
}