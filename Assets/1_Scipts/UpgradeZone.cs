using UnityEngine;
using UnityEngine.Events;
using static GameEnums;
using static Interfaces;

public class UpgradeZone : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public int requiredCash = 100;
    public int currentCash = 0;
    public ResourceType payResourceType = ResourceType.Cash;

    [Header("Events")]
    public UnityEvent onUpgradeComplete; // 업그레이드 완료 시 실행할 이벤트

    [Header("Interaction")]
    public float interactCooldown = 0.1f; // 돈이 빠져나가는 간격
    private float lastInteractTime;

    public void Interact(PlayerInteraction player)
    {
        if (currentCash >= requiredCash) return;
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time;

        // 1. 우선 플레이어 등에 있는 물리적 돈부터 소모 (시각적 재미)
        if (player.playerStack.HasResourceType(ResourceType.Cash))
        {
            ResourceItem cashItem = player.playerStack.RemoveSpecificType(ResourceType.Cash);
            if (cashItem != null)
            {
                ConsumeResource(cashItem);
                return;
            }
        }

        // 2. 등에 돈이 없다면 가상 재화(GameManager)에서 직접 소모
        if (GameManager.Instance.CurrentMoney >= 10) // 최소 단위가 10원일 때
        {
            if (GameManager.Instance.SpendMoney(10))
            {
                currentCash += 10;
                // (선택) 가상 재화가 소모될 때도 돈 입자가 날아가는 UI 연출 등을 넣으면 좋음
                //CheckUpgradeComplete();
            }
        }
    }

    private void ConsumeResource(ResourceItem item)
    {
        // 돈이 구역 중앙으로 빨려 들어가는 연출 (0.2초)
        item.JumpTo(transform, Vector3.zero, 0.2f, () =>
        {
            // 돈의 가치만큼 현재 금액 추가 (예: 개당 10원)
            // ResourceItem에 value 변수가 있다면 item.value를 사용하세요.
            currentCash += 1;

            // 연출이 끝난 오브젝트는 파괴 (돈은 풀링 안 하기로 함)
            ObjectPool.Instance.Push(item.gameObject);

            // 완료 체크
            if (currentCash >= requiredCash)
            {
                onUpgradeComplete?.Invoke();
                gameObject.SetActive(false); // 구역 비활성화
            }
        });
    }
}