using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class BookingDesk : MonoBehaviour
{
    [Header("Stacks")]
    public MachineStackHandler inputStack;  // 수갑이 쌓이는 곳
    public MachineStackHandler outputStack; // 돈이 쌓이는 곳
    public GameObject cashPrefab;

    [Header("Queue System")]
    public Transform queueStartPoint;       // 업무 보는 위치
    public Transform[] queuePoints;         // 대기 줄 위치들
    public Transform jailEntrancePoint;     // 감옥 입구
    public Transform spawnPoint;            // NPC 스폰 지점

    [Header("Settings")]
    public GameObject npcPrefab;
    public float spawnInterval = 3f;
    public CriminalDataSO criminalData;

    [Header("Door Object")]
    public GameObject jailDoor;             // 감옥 문 오브젝트
    private int travelingPrisonersCount = 0; // 현재 감옥으로 이동 중인 죄수 수

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

        // NPC가 업무 데스크 위치에 도착했는지 확인
        if (Vector3.Distance(currentProcessingNPC.transform.position, queueStartPoint.position) > 0.5f) return;

        // 수갑이 충분하고 돈을 쌓을 공간이 있으면 시작
        if (outputStack.Count < 100 && inputStack.Count >= criminalData.requiredHandcuffs)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    private IEnumerator ConvertRoutine()
    {
        isProcessing = true;

        // 1. 수갑 소모 연출
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

        // 2. 죄수로 변환
        currentProcessingNPC.ChangeToPrisoner();

        // 3. 보상(돈) 생성
        int reward = criminalData.rewardCash;
        for (int i = 0; i < reward; i++)
        {
            GameObject cashObj = ObjectPool.Instance.Pop(cashPrefab, currentProcessingNPC.transform.position + Vector3.up, Quaternion.identity);
            ResourceItem cashItem = cashObj.GetComponent<ResourceItem>();
            outputStack.AddToStack(cashItem, 100);
            yield return new WaitForSeconds(0.05f);
        }

        // 4. 감옥으로 이동 및 문 제어
        travelingPrisonersCount++;
        if (jailDoor != null) jailDoor.SetActive(false); // 문 열림

        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        StartCoroutine(currentProcessingNPC.GoToJail(jailEntrancePoint.position + randomOffset, () =>
        {
            // 감옥 도착 시 콜백
            travelingPrisonersCount--;

            // 이동 중인 죄수가 더 이상 없으면 문 닫기
            if (travelingPrisonersCount <= 0)
            {
                travelingPrisonersCount = 0;
                if (jailDoor != null) jailDoor.SetActive(true); // 문 닫힘
            }
        }));

        yield return new WaitForSeconds(0.5f);

        currentProcessingNPC = null;
        isProcessing = false;
    }

    // --- 상호작용: 플레이어가 수갑을 줄 때 ---
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

    // --- 상호작용: 플레이어가 돈을 수거할 때 ---
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
                    int cashValue = 5; // [규칙] 돈 한 장의 가치는 5원

                    if (player.playerStack.CanAdd(ResourceType.Cash))
                    {
                        // 1. 가방에 담기
                        player.playerStack.AddToStack(item, 0);
                        // 2. 지갑(UI)에 즉시 반영
                        GameManager.Instance.AddMoney(cashValue);
                    }
                    else
                    {
                        // 가방이 꽉 찼을 때 먹으면 바로 돈으로 환산 연출
                        item.JumpTo(player.transform, Vector3.up * 2f, 0.2f, () => {
                            GameManager.Instance.AddMoney(cashValue);
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
                    // 가방에 못 담으면 다시 스택에 돌려놓음
                    outputStack.AddToStack(item, 100);
                }
            }
        }
    }
}