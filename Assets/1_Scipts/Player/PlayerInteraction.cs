using System.Collections;
using UnityEngine;
using static GameEnums;
using static Interfaces;

public class PlayerInteraction : MonoBehaviour
{
    public PlayerStackHandler playerStack;
    public GameObject orePrefab; // 임시 채굴용 프리팹 (보통 돌에서 생성)

    public EquipmentDataSO currentEqData;

    private float lastMineTime;
    private IMineable currentTargetRock;
    private bool isMining = false; // 채굴 상태

    // 문자열 대신 해시값을 사용하여 성능 최적화
    private readonly int isMovingHash = Animator.StringToHash("isMoving");
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
        if (!playerStack.CanAdd(ResourceType.IronOre)) return;

        isMining = true;
        lastMineTime = Time.time;
        GetComponent<Animator>().SetTrigger(doMiningHash);
    }

    // 1. 애니메이션 이벤트 수신: 곡괭이가 돌을 내리찍는 정확한 순간에 호출됨
    public void OnMiningImpact()
    {
        if (currentTargetRock != null && currentTargetRock.IsActive)
        {
            // [수정] 채굴 전 가방에 자리가 있는지 확인 (타입 지정)
            if (!playerStack.CanAdd(ResourceType.IronOre)) return;

            // 여기서 실제로 체력을 깎고 파편을 생성합니다.
            // 이펙트 및 데미지 전달
            currentTargetRock.TakeDamage(currentEqData.mineDamage, ((Component)currentTargetRock).transform.position);
            // 채굴 성공 시 광물 파편 생성 후 등 뒤로 날아감
            GameObject oreObj = ObjectPool.Instance.Pop(orePrefab, ((Component)currentTargetRock).transform.position, Quaternion.identity);
            ResourceItem item = oreObj.GetComponent<ResourceItem>();

            // 혹시 ResourceItem을 안 붙였을 때를 대비한 안전장치(Null 체크)
            if (item != null)
            {
                playerStack.AddToStack(item, currentEqData.maxCapacity);
            }
        }
    }

    // 2. 애니메이션 이벤트 수신: 채굴 모션이 끝날 때 호출됨
    public void FinishMining()
    {
        isMining = false;
        currentTargetRock = null;
    }
}