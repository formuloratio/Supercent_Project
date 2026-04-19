using UnityEngine;

public class MiningZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 들어오면 장비 활성화
        if (other.CompareTag("Player"))
        {
            var handler = other.GetComponent<PlayerEquipmentHandler>();
            if (handler != null)
            {
                handler.SetEquipmentVisibility(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 나가면 장비 비활성화
        if (other.CompareTag("Player"))
        {
            var handler = other.GetComponent<PlayerEquipmentHandler>();
            if (handler != null)
            {
                handler.SetEquipmentVisibility(false);
            }
        }
    }
}