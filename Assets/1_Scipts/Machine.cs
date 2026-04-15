using System.Collections;
using UnityEngine;

public class Machine : MonoBehaviour
{
    public StackHandler cuffDisplayStack; // 수갑이 쌓일 장소
    public GameObject handcuffPrefab;
    private int pendingOres = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(TakeOreFromPlayer(other.GetComponent<PlayerController>()));
        }
    }

    IEnumerator TakeOreFromPlayer(PlayerController player)
    {
        while (player.oreStack.Count > 0)
        {
            GameObject ore = player.oreStack.RemoveStack();
            ObjectPool.Instance.Push(ore);
            pendingOres++;
            yield return new WaitForSeconds(0.1f);
        }
        if (pendingOres > 0) StartCoroutine(ProcessHandcuffs());
    }

    IEnumerator ProcessHandcuffs()
    {
        while (pendingOres > 0)
        {
            pendingOres--;
            yield return new WaitForSeconds(1f);
            SpawnHandcuff();
        }
    }

    private void SpawnHandcuff()
    {
        // 수갑은 무제한으로 쌓인다고 가정하거나 적절한 capacity 전달
        cuffDisplayStack.AddStack(handcuffPrefab, 999);
    }
}