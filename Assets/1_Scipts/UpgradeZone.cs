using TMPro;
using UnityEngine;
using static GameEnums;
using static Interfaces; // IInteractable이 정의된 네임스페이스 (필요시 확인)

public enum UpgradeType { Equipment, MinerNPC, TransporterNPC }

// [핵심] IInteractable을 반드시 상속받아야 Player가 인식합니다!
public class UpgradeZone : MonoBehaviour, IInteractable
{
    [Header("Progress")]
    public int currentCash = 0;
    private int requiredCash = -1;

    private float lastInteractTime;
    private float interactCooldown = 0.05f;

    [Header("UI Reference")]
    public TextMeshProUGUI priceText;

    [Header("Upgrade Settings")]
    public UpgradeType upgradeType;
    public EquipmentDataSO equipmentData; // 장비용
    public GameObject npcPrefab;          // NPC용
    public int spawnCount = 3;            // 생성할 NPC 수

    private void Start()
    {
        if (equipmentData != null)
        {
            requiredCash = equipmentData.price;
        }

        if (priceText == null)
            priceText = GetComponentInChildren<TextMeshProUGUI>();

        UpdatePriceText();
    }

    // [중요] PlayerInteraction에서 호출하는 인터페이스 함수
    public void Interact(PlayerInteraction player)
    {
        // 작동 여부 확인을 위해 디버그 로그 추가
        // Debug.Log("[UpgradeZone] 플레이어와 접촉 중...");

        if (requiredCash <= 0 || currentCash >= requiredCash) return;
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time;

        // 1. 물리적 돈 소모
        if (player.playerStack.HasResourceType(ResourceType.Cash))
        {
            ResourceItem cashItem = player.playerStack.RemoveSpecificType(ResourceType.Cash);
            if (cashItem != null)
            {
                ConsumePhysicalResource(cashItem, player);
                return;
            }
        }

        // 2. 가상 재화 소모 (등에 돈이 없을 때)
        int spendAmount = 5;
        if (GameManager.Instance.SpendMoney(spendAmount))
        {
            currentCash += spendAmount;
            UpdatePriceText();
            CheckUpgradeComplete(player);
        }
    }

    private void ConsumePhysicalResource(ResourceItem item, PlayerInteraction player)
    {
        item.JumpTo(transform, Vector3.zero, 0.2f, () =>
        {
            int cashValue = 5;
            GameManager.Instance.SpendMoney(cashValue);

            currentCash += cashValue;
            UpdatePriceText();
            ObjectPool.Instance.Push(item.gameObject);
            CheckUpgradeComplete(player);
        });
    }

    private void UpdatePriceText()
    {
        if (priceText != null)
        {
            int remaining = requiredCash - currentCash;
            priceText.text = remaining > 0 ? remaining.ToString() : "0";
        }
    }

    private void CheckUpgradeComplete(PlayerInteraction player)
    {
        if (currentCash >= requiredCash)
        {
            switch (upgradeType)
            {
                case UpgradeType.Equipment:
                    player.GetComponent<PlayerController>().ChangeEquipment(equipmentData);
                    break;

                case UpgradeType.MinerNPC:
                case UpgradeType.TransporterNPC:
                    for (int i = 0; i < spawnCount; i++)
                    {
                        // 발판 앞에서 약간의 간격을 두고 생성
                        Vector3 spawnPos = transform.position + transform.forward * 2f + new Vector3(i, 0, 0);
                        Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                    }
                    break;
            }
            gameObject.SetActive(false); // 발판 제거
        }
    }
}