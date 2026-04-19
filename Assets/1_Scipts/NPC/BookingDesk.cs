using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class BookingDesk : MonoBehaviour
{
    [Header("Stacks")]
    public MachineStackHandler inputStack;
    public MachineStackHandler outputStack;
    public GameObject cashPrefab;

    [Header("Queue System")]
    public Transform queueStartPoint;
    public Transform[] queuePoints;
    public Transform jailEntrancePoint;
    public Transform spawnPoint;

    [Header("Settings")]
    public GameObject npcPrefab;
    public float spawnInterval = 3f;
    public CriminalDataSO criminalData;

    [Header("Door Object")]
    public GameObject jailDoor; // 감옥 문 오브젝트 연결
    private int travelingPrisonersCount = 0; // 현재 감옥으로 걸어가고 있는 죄수 수

    private List<CriminalNPC> waitingQueue = new List<CriminalNPC>();
    private CriminalNPC currentProcessingNPC;

    private bool isProcessing = false;
    private float spawnTimer;

    private float lastCollectTime;
    private bool isTransferring = false;

    private void Update()
    {
        HandleSpawning();
        ProcessQueue();
        CheckAndProcessCriminal();
    }

    private void HandleSpawning()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && waitingQueue.Count < queuePoints.Length)
        {
            spawnTimer = 0f;
            SpawnNPC();
        }
    }

    private void SpawnNPC()
    {
        if (spawnPoint == null || npcPrefab == null) return;

        GameObject npcObj = ObjectPool.Instance.Pop(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        CriminalNPC npc = npcObj.GetComponent<CriminalNPC>();

        waitingQueue.Add(npc);
    }

    private void ProcessQueue()
    {
        if (currentProcessingNPC == null && waitingQueue.Count > 0)
        {
            currentProcessingNPC = waitingQueue[0];
            waitingQueue.RemoveAt(0);
            currentProcessingNPC.MoveToTarget(queueStartPoint);
        }

        for (int i = 0; i < waitingQueue.Count; i++)
        {
            waitingQueue[i].MoveToTarget(queuePoints[i]);
        }
    }

    private void CheckAndProcessCriminal()
    {
        if (isProcessing || currentProcessingNPC == null || criminalData == null) return;
        if (Vector3.Distance(currentProcessingNPC.transform.position, queueStartPoint.position) > 0.5f) return;

        if (outputStack.Count < 100 && inputStack.Count >= criminalData.requiredHandcuffs)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    private IEnumerator ConvertRoutine()
    {
        isProcessing = true;

        int required = criminalData.requiredHandcuffs;
        for (int i = 0; i < required; i++)
        {
            ResourceItem handcuff = inputStack.RemoveFromStack();
            if (handcuff != null)
            {
                handcuff.JumpTo(currentProcessingNPC.transform, Vector3.up * 1.5f, 0.3f, () =>
                {
                    ObjectPool.Instance.Push(handcuff.gameObject);
                });
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(0.3f);

        currentProcessingNPC.ChangeToPrisoner();

        int reward = criminalData.rewardCash;
        for (int i = 0; i < reward; i++)
        {
            GameObject cashObj = ObjectPool.Instance.Pop(cashPrefab, currentProcessingNPC.transform.position + Vector3.up, Quaternion.identity);
            ResourceItem cashItem = cashObj.GetComponent<ResourceItem>();
            outputStack.AddToStack(cashItem, 100);
            yield return new WaitForSeconds(0.05f);
        }

        // 1. 이동 중인 죄수 카운트 증가 및 문 열기(비활성화)
        travelingPrisonersCount++;
        if (jailDoor != null) jailDoor.SetActive(false);

        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));

        // 2. GoToJail 실행 시 도착 시 실행할 함수(콜백)를 넘겨줌
        StartCoroutine(currentProcessingNPC.GoToJail(jailEntrancePoint.position + randomOffset, () =>
        {
            // 도착 시 실행될 내용
            travelingPrisonersCount--;

            // 3. 더 이상 이동 중인 죄수가 없으면 문 닫기(활성화)
            if (travelingPrisonersCount <= 0)
            {
                travelingPrisonersCount = 0; // 음수 방지 안전장치
                if (jailDoor != null) jailDoor.SetActive(true);
            }
        }));

        // 3. 죄수가 감옥으로 출발했으므로, 약간의 텀(0.5초)을 두고 다음 사람을 부릅니다.
        yield return new WaitForSeconds(0.5f);

        // 4. 상태를 초기화하여 다음 NPC가 queueStartPoint로 올 수 있게 합니다.
        currentProcessingNPC = null;
        isProcessing = false;
    }

    // --- 상호작용 함수 ---
    public void Deposit(PlayerInteraction player)
    {
        if (isTransferring) return;
        StartCoroutine(DepositRoutine(player));
    }

    private IEnumerator DepositRoutine(PlayerInteraction player)
    {
        isTransferring = true;
        while (player.playerStack.HasResourceType(ResourceType.Handcuff))
        {
            ResourceItem item = player.playerStack.RemoveSpecificType(ResourceType.Handcuff);
            if (item != null)
            {
                inputStack.AddToStack(item, 100);
                yield return new WaitForSeconds(0.05f);
            }
            else break;
        }
        isTransferring = false;
    }

    public void Collect(PlayerInteraction player)
    {
        if (Time.time - lastCollectTime < 0.05f) return;

        if (outputStack.Count > 0)
        {
            lastCollectTime = Time.time;
            ResourceItem item = outputStack.RemoveFromStack();

            if (item != null)
            {
                if (item.type == ResourceType.Cash)
                {
                    if (player.playerStack.CanAdd(ResourceType.Cash))
                    {
                        player.playerStack.AddToStack(item, 0);
                    }
                    else
                    {
                        item.JumpTo(player.transform, Vector3.up * 2f, 0.2f, () => {
                            GameManager.Instance.AddMoney(10);
                            ObjectPool.Instance.Push(item.gameObject);
                        });
                    }
                }
                else if (player.playerStack.CanAdd(item.type))
                {
                    player.playerStack.AddToStack(item, 0);
                }
                else
                {
                    outputStack.AddToStack(item, 100);
                }
            }
        }
    }
}