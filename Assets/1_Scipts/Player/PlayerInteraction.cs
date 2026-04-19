using System.Collections;
using UnityEngine;
using static GameEnums;
using static Interfaces;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerStackHandler playerStack;
    public GameObject orePrefab; // 임시 채굴용 프리팹 (보통 돌에서 생성)

    [Header("UI Feedback")]
    public GameObject maxTextPrefab; // "MAX"라고 적힌 팝업 UI 혹은 프리팹

    public EquipmentDataSO currentEqData;

    private float lastMineTime;
    private IMineable currentTargetRock;
    private bool isMining = false; // 채굴 상태

    // 문자열 대신 해시값을 사용하여 성능 최적화
    private readonly int doMiningHash = Animator.StringToHash("doMining");

    public void UpdateEquipmentData(EquipmentDataSO data)
    {
        currentEqData = data;
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. 채굴
        if (other.CompareTag("IronStone"))
        {
            currentTargetRock = other.GetComponent<IMineable>();

            if (!isMining)
            {
                if (currentTargetRock != null && currentTargetRock.IsActive)
                {
                    if (Time.time - lastMineTime >= currentEqData.mineCooldown)
                    {
                        PerformMining();
                    }
                }
            }
        }

        // 2. 기계 및 구역 상호작용
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact(this);
        }
    }

    // 수정된 PerformMining 함수 (애니메이션만 실행)
    private void PerformMining()
    {
        // [수정] 전체 개수가 아니라 "철광석" 자리가 있는지 확인합니다.
        //if (!playerStack.CanAdd(ResourceType.IronOre)) return;

        isMining = true;
        lastMineTime = Time.time;
        GetComponent<Animator>().SetTrigger(doMiningHash);
    }

    // 1. 애니메이션 이벤트 수신: 곡괭이가 돌을 내리찍는 정확한 순간에 호출됨
    public void OnMiningImpact()
    {
        if (currentTargetRock != null && currentTargetRock.IsActive)
        {
            // [핵심] 여기서 가방이 꽉 찼는지 확인합니다.
            if (!playerStack.CanAdd(ResourceType.IronOre))
            {
                // 가방이 꽉 찼을 때: MAX 표시만 띄움
                ShowMaxFeedback(((Component)currentTargetRock).transform.position + Vector3.up * 2f);
                return;
            }

            // 가방에 자리가 있을 때만: 실제 채굴 로직 실행
            currentTargetRock.TakeDamage(currentEqData.mineDamage, ((Component)currentTargetRock).transform.position);

            GameObject oreObj = ObjectPool.Instance.Pop(orePrefab, ((Component)currentTargetRock).transform.position, Quaternion.identity);
            ResourceItem item = oreObj.GetComponent<ResourceItem>();

            if (item != null)
            {
                playerStack.AddToStack(item, currentEqData.maxCapacity);
            }
        }
    }

    private void ShowMaxFeedback(Vector3 position)
    {
        // 방법 1: 간단하게 로그만 찍거나
        // Debug.Log("가방이 가득 찼습니다!");

        // 방법 2: MAX 프리팹을 띄우는 로직 (추천)
        if (maxTextPrefab != null)
        {
            // ObjectPool을 사용하여 MAX 표시 오브젝트를 팝업
            ObjectPool.Instance.Pop(maxTextPrefab, position, Quaternion.identity);

            // 일정 시간 후 다시 풀에 넣는 로직이 포함된 스크립트가 maxPop에 있으면 좋습니다.
        }
    }

    // 2. 애니메이션 이벤트 수신: 채굴 모션이 끝날 때 호출됨
    public void FinishMining()
    {
        isMining = false;
        currentTargetRock = null;
    }
}