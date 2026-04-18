using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class BookingDesk : MonoBehaviour
{
    [Header("Stacks")]
    public MachineStackHandler inputStack;  // 수갑이 놓일 곳 (In_Zone)
    public MachineStackHandler outputStack; // 돈이 생성될 곳 (Out_Zone)
    public GameObject cashPrefab;           // 생성될 돈 프리팹

    [Header("Queue System")]
    public Transform queueStartPoint;       // 심사대 바로 앞 (처리 받는 위치)
    public Transform[] queuePoints;         // 대기줄 위치들
    public Transform jailEntrancePoint;     // 죄수가 향할 감옥 목적지

    [Header("Spawn Settings")]
    public GameObject npcPrefab;            // NPC 기본 껍데기 프리팹
    public CriminalDataSO[] spawnableCriminals; // 스폰 가능한 SO 리스트
    public float spawnInterval = 3f;

    private List<CriminalNPC> waitingQueue = new List<CriminalNPC>();
    private CriminalNPC currentProcessingNPC;

    private bool isProcessing = false;
    private float spawnTimer;

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
        // 랜덤한 SO 데이터 선택
        CriminalDataSO randomData = spawnableCriminals[Random.Range(0, spawnableCriminals.Length)];

        // NPC 생성 (화면 밖이나 특정 스폰 포인트에서)
        GameObject npcObj = ObjectPool.Instance.Pop(npcPrefab, queuePoints[queuePoints.Length - 1].position, Quaternion.identity);
        CriminalNPC npc = npcObj.GetComponent<CriminalNPC>();

        npc.Initialize(randomData);
        waitingQueue.Add(npc);
    }

    private void ProcessQueue()
    {
        // 현재 심사 중인 사람이 없고, 대기열에 사람이 있다면
        if (currentProcessingNPC == null && waitingQueue.Count > 0)
        {
            currentProcessingNPC = waitingQueue[0];
            waitingQueue.RemoveAt(0);
            currentProcessingNPC.MoveToTarget(queueStartPoint.position);
        }

        // 나머지 대기열 사람들 앞으로 한 칸씩 이동
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            waitingQueue[i].MoveToTarget(queuePoints[i].position);
        }
    }

    private void CheckAndProcessCriminal()
    {
        if (isProcessing || currentProcessingNPC == null) return;

        // 심사대에 NPC가 도착했는지 확인
        if (Vector3.Distance(currentProcessingNPC.transform.position, queueStartPoint.position) > 0.5f) return;

        // 아웃 스택에 자리가 있고, 인 스택에 요구하는 수갑 개수만큼 있는지 확인
        if (outputStack.Count < 100 && inputStack.Count >= currentProcessingNPC.currentData.requiredHandcuffs)
        {
            StartCoroutine(ConvertRoutine());
        }
    }

    private IEnumerator ConvertRoutine()
    {
        isProcessing = true;
        int required = currentProcessingNPC.currentData.requiredHandcuffs;

        // 1. 수갑을 NPC에게 날려보냄
        for (int i = 0; i < required; i++)
        {
            ResourceItem handcuff = inputStack.RemoveFromStack();
            if (handcuff != null)
            {
                // NPC의 가슴 높이쯤으로 점프 (로컬 Y: 1.5f 정도)
                handcuff.JumpTo(currentProcessingNPC.transform, Vector3.up * 1.5f, 0.3f, () =>
                {
                    ObjectPool.Instance.Push(handcuff.gameObject);
                });
                yield return new WaitForSeconds(0.1f); // 연속으로 날아가는 연출 간격
            }
        }

        // 수갑이 다 날아갈 때까지 잠깐 대기
        yield return new WaitForSeconds(0.3f);

        // 2. NPC를 죄수로 변환 (옷 갈아입기)
        currentProcessingNPC.ChangeToPrisoner();

        // 3. 돈(Cash) 생성해서 Output 존에 쌓기
        int reward = currentProcessingNPC.currentData.rewardCash;
        for (int i = 0; i < reward; i++)
        {
            GameObject cashObj = ObjectPool.Instance.Pop(cashPrefab, currentProcessingNPC.transform.position + Vector3.up, Quaternion.identity);
            ResourceItem cashItem = cashObj.GetComponent<ResourceItem>();
            outputStack.AddToStack(cashItem, 100);
            yield return new WaitForSeconds(0.05f); // 돈이 타다닥 쌓이는 연출
        }

        // 4. 죄수를 감옥으로 쫓아냄
        currentProcessingNPC.GoToJail(jailEntrancePoint.position);

        // 5. 다음 사람 받을 준비
        currentProcessingNPC = null;
        isProcessing = false;
    }
}